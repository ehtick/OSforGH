﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.Drawing;
using System.Windows.Forms;
///for GUI*********************************
using Grasshopper.Kernel.Attributes;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
///****************************************
using System.Diagnostics;

using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp;
using System.Security.Permissions;
using Rhino;

namespace CLTCheckPDF
{
    public class CLTCheckPDF : GH_Component
    {
        public static int on_off = 0;
        public static PdfCreate.JapaneseFontResolver fontresolver = new PdfCreate.JapaneseFontResolver();
        public static XGraphics gfx;
        public static void SetButton(string s, int i)
        {
            if (s == "1")
            {
                on_off = i;
            }
        }
        public CLTCheckPDF()
          : base("CLTCheck PDF Output", "CLTCheck PDF Output",
              "Output CLTCheck result to pdf",
              "OpenSees", "Utility")
        {
        }
        public override void CreateAttributes()
        {
            m_attributes = new CustomGUI(this);
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("index", "index", "[...](element No. List to show)", GH_ParamAccess.list, -9999);///
            pManager.AddNumberParameter("element_node_relationship", "IJ", "[[No.i,No.j,material No.,section No.,angle],...](DataTree)", GH_ParamAccess.tree);///
            pManager.AddNumberParameter("lambda", "lambda", "elongation ratio[...](DataList)", GH_ParamAccess.list);///
            pManager.AddNumberParameter("section area", "A", "[...](DataList)", GH_ParamAccess.list);///
            pManager.AddNumberParameter("Section modulus around y-axis", "Zy", "[...](DataList)", GH_ParamAccess.list);///
            pManager.AddNumberParameter("Section modulus around z-axis", "Zz", "[...](DataList)", GH_ParamAccess.list);///
            pManager.AddNumberParameter("fk", "fk", "[...](DataList)[N/mm2]", GH_ParamAccess.tree);///
            pManager.AddNumberParameter("ft", "ft", "[...](DataList)[N/mm2]", GH_ParamAccess.tree);///
            pManager.AddNumberParameter("fb(inner)", "fb(inner)", "[...](DataList)[N/mm2]", GH_ParamAccess.tree);///
            pManager.AddNumberParameter("fs(inner)", "fs(inner)", "[...](DataList)[N/mm2]", GH_ParamAccess.tree);///
            pManager.AddNumberParameter("fb(outer)", "fb(outer)", "[...](DataList)[N/mm2]", GH_ParamAccess.tree);///
            pManager.AddNumberParameter("fs(outer)", "fs(outer)", "[...](DataList)[N/mm2]", GH_ParamAccess.tree);///
            pManager.AddNumberParameter("safe factor", "alpha", "Reduction rate taking into account cross-sectional defects", GH_ParamAccess.item);///
            pManager.AddNumberParameter("sectional_force", "sec_f", "[[Pxi,Pyi,Pzi,Mxi,Myi,Mzi,Pxj,Pyj,Pzj,Mxj,Myj,Mzj,Pxc,Pyc,Pzc,Mxc,Myc,Mzc],...](DataTree)", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("kentei_hi", "kentei", "[[for Ni,for Qyi,for Qzi,for Myi,for Mzi,for Nj,for Qyj,for Qzj,for Myj,for Mzj,for Nc,for Qyc,for Qzc,for Myc,for Mzc],...](DataTree)", GH_ParamAccess.tree);///
            pManager.AddTextParameter("secname", "secname", "section name", GH_ParamAccess.list, "");///
            pManager.AddNumberParameter("burnB", "burnB", "[double,double,...](Datalist)[m]", GH_ParamAccess.list);///
            pManager.AddNumberParameter("burnD", "burnD", "[double,double,...](Datalist)[m]", GH_ParamAccess.list);///
            pManager.AddTextParameter("outputname", "outputname", "output file name", GH_ParamAccess.item, "CLTCheck");///
            pManager[16].Optional = true; pManager[17].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (on_off == 1)
            {
                var index = new List<double>(); DA.GetDataList("index", index);
                DA.GetDataTree("element_node_relationship", out GH_Structure<GH_Number> _ij); var ij = _ij.Branches;
                DA.GetDataTree("sectional_force", out GH_Structure<GH_Number> _sec_f);
                DA.GetDataTree("fk", out GH_Structure<GH_Number> _fk); DA.GetDataTree("ft", out GH_Structure<GH_Number> _ft);
                DA.GetDataTree("fb(inner)", out GH_Structure<GH_Number> _fbi); DA.GetDataTree("fs(inner)", out GH_Structure<GH_Number> _fsi);
                DA.GetDataTree("fb(outer)", out GH_Structure<GH_Number> _fbo); DA.GetDataTree("fs(outer)", out GH_Structure<GH_Number> _fso);
                List<double> Lambda = new List<double>(); DA.GetDataList("lambda", Lambda);
                List<double> A = new List<double>(); DA.GetDataList("section area", A);
                List<double> Zy = new List<double>(); DA.GetDataList("Section modulus around y-axis", Zy);
                List<double> Zz = new List<double>(); DA.GetDataList("Section modulus around z-axis", Zz);
                var fk = _fk.Branches[0]; var ft = _ft.Branches[0]; var fbi = _fbi.Branches[0]; var fsi = _fsi.Branches[0]; var fbo = _fbo.Branches[0]; var fso = _fso.Branches[0];
                var fk2 = _fk.Branches[0]; var ft2 = _ft.Branches[0]; var fbi2 = _fbi.Branches[0]; var fsi2 = _fsi.Branches[0]; var fbo2 = _fbo.Branches[0]; var fso2 = _fso.Branches[0];
                if (_sec_f.Branches.Count / ij.Count == 5)
                {
                    fk2 = _fk.Branches[1]; ft2 = _ft.Branches[1]; fbi2 = _fbi.Branches[1]; fsi2 = _fsi.Branches[1]; fbo2 = _fbo.Branches[1]; fso2 = _fso.Branches[1];
                }
                double alpha = 1.0; DA.GetData("safe factor", ref alpha);
                DA.GetDataTree("kentei_hi", out GH_Structure<GH_Number> _kentei);
                List<string> secname = new List<string>(); DA.GetDataList("secname", secname); var pdfname = "CLTCheck"; DA.GetData("outputname", ref pdfname);
                int Digit(int num)//数字の桁数を求める関数
                {
                    // Mathf.Log10(0)はNegativeInfinityを返すため、別途処理する。
                    return (num == 0) ? 1 : ((int)Math.Log10(num) + 1);
                }
                XColor RGB(double h, double s, double l)//convert HSL to RGB
                {
                    var max = 0.0; var min = 0.0; var r = 0.0; var g = 0.0; var b = 0.0;
                    if (l < 0.5)
                    {
                        max = l + l * s;
                        min = l - l * s;
                    }
                    else
                    {
                        max = l + (1 - l) * s;
                        min = l - (1 - l) * s;
                    }
                    var HUE_MAX = 360.0; var RGB_MAX = 255;
                    var hp = HUE_MAX / 6.0; h *= HUE_MAX; var q = h / hp;
                    if (q <= 1)
                    {
                        r = max;
                        g = (h / hp) * (max - min) + min;
                        b = min;
                    }
                    else if (q <= 2)
                    {
                        r = ((hp * 2 - h) / hp) * (max - min) + min;
                        g = max;
                        b = min;
                    }
                    else if (q <= 3)
                    {
                        r = min;
                        g = max;
                        b = ((h - hp * 2) / hp) * (max - min) + min;
                    }
                    else if (q <= 4)
                    {
                        r = min;
                        g = ((hp * 4 - h) / hp) * (max - min) + min;
                        b = max;
                    }
                    else if (q <= 5)
                    {
                        r = ((h - hp * 4) / hp) * (max - min) + min;
                        g = min;
                        b = max;
                    }
                    else
                    {
                        r = max;
                        g = min;
                        b = ((HUE_MAX - h) / hp) * (max - min) + min;
                    }
                    r *= RGB_MAX; g *= RGB_MAX; b *= RGB_MAX;
                    return XColor.FromArgb((int)r, (int)g, (int)b);
                }
                if (index[0] == -9999)
                {
                    index = new List<double>();
                    for (int e = 0; e < ij.Count; e++) { index.Add(e); }
                }
                // フォントリゾルバーのグローバル登録
                if (PdfCreate.JapaneseFontResolver.fontset == 0) { PdfSharp.Fonts.GlobalFontSettings.FontResolver = fontresolver; PdfCreate.JapaneseFontResolver.fontset = 1; }
                // PDFドキュメントを作成。
                PdfDocument document = new PdfDocument();
                document.Info.Title = pdfname;
                document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu";
                // フォントを作成。
                XFont font = new XFont("Gen Shin Gothic", 9, XFontStyle.Regular);
                XFont fontbold = new XFont("Gen Shin Gothic", 9, XFontStyle.Bold);
                var pen = XPens.Black;
                var burnB = new List<double>(); var burnD = new List<double>(); if (!DA.GetDataList("burnB", burnB)) { }; if (!DA.GetDataList("burnD", burnD)) { };
                var labels = new List<string>
                {
                    "部材番号","部材断面","λ","A[cm2]","α(低減率)","Zy[cm3]","Zz[cm3]","","fk[N/mm2]", "ft[N/mm2]", "fb(面内)[N/mm2]", "fs(面内)[N/mm2]", "fb(面外)[N/mm2]", "fs(面外)[N/mm2]","", "節点番号","", "N[kN]","My[kNm]", "Mz[kNm]","Qy[kN]","Qz[kN]","軸+曲げ検定比","せん断検定比","判定",
                    "", "N[kN]","My[kNm]", "Mz[kNm]","Qy[kN]","Qz[kN]","軸+曲げ検定比","せん断検定比","判定",
                    "", "N[kN]","My[kNm]", "Mz[kNm]","Qy[kN]","Qz[kN]","軸+曲げ検定比","せん断検定比","判定",
                    "", "N[kN]","My[kNm]", "Mz[kNm]","Qy[kN]","Qz[kN]","軸+曲げ検定比","せん断検定比","判定",
                    "", "N[kN]","My[kNm]", "Mz[kNm]","Qy[kN]","Qz[kN]","軸+曲げ検定比","せん断検定比","判定"
                };
                if (burnB.Count != 0 && burnD.Count != 0)
                {
                    labels = new List<string>
                    {
                    "部材番号","部材断面","燃えしろB[mm]","燃えしろD[mm]","λ","A[cm2]","α(低減率)","Zy[cm3]","Zz[cm3]","","fk[N/mm2]", "ft[N/mm2]", "fb[N/mm2]", "fs[N/mm2]","", "節点番号","", "N[kN]","My[kNm]", "Mz[kNm]","Qy[kN]","Qz[kN]","軸+曲げ検定比","せん断検定比","判定"
                    };
                }
                var label_width = 75; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 25; PdfPage page = new PdfPage(); page.Size = PageSize.A4;
                for (int e = 0; e < index.Count; e++)//
                {
                    int ni = (int)ij[e][0].Value; int nj = (int)ij[e][1].Value; int sec = (int)ij[e][3].Value;
                    var ele_text = ((int)index[e]).ToString(); var ni_text = ni.ToString(); var nj_text = nj.ToString();
                    var name_text = secname[sec]; var lambda_text = Lambda[e].ToString("F").Substring(0, Digit((int)Lambda[e]));
                    var A_text = (A[sec] * 1e+4).ToString("F").Substring(0, Digit((int)(A[sec] * 1e+4)));
                    var b = 0.0; var d = 0.0;
                    if (burnB.Count != 0 && burnD.Count != 0) { b = Math.Pow(6 * Math.Pow(Zz[sec], 2) / Zy[sec], 1.0 / 3.0); d = Math.Pow(6 * Math.Pow(Zy[sec], 2) / Zz[sec], 1.0 / 3.0); }
                    if (burnB.Count != 0 && burnD.Count != 0)
                    {
                        A_text = (b * d * 1e+4).ToString("F").Substring(0, Digit((int)(b * d * 1e+4)));
                    }
                    var alpha_text = alpha.ToString("F"); alpha_text = alpha_text.Substring(0, Math.Min(4, alpha_text.Length));
                    var Zy_text = (Zy[sec] * 1e+6).ToString("F"); Zy_text = Zy_text.Substring(0, Math.Min(Digit((int)(Zy[sec] * 1e+6)), Zy_text.Length));
                    var Zz_text = (Zz[sec] * 1e+6).ToString("F"); Zz_text = Zz_text.Substring(0, Math.Min(Digit((int)(Zz[sec] * 1e+6)), Zz_text.Length));
                    if (burnB.Count != 0 && burnD.Count != 0)
                    {
                        var zy = b * Math.Pow(d, 2) / 6.0; var zz = d * Math.Pow(b, 2) / 6.0;
                        Zy_text = (zy * 1e+6).ToString("F"); Zy_text = Zy_text.Substring(0, Math.Min(Digit((int)(zy * 1e+6)), Zy_text.Length));
                        Zz_text = (zz * 1e+6).ToString("F"); Zy_text = Zy_text.Substring(0, Math.Min(Digit((int)(zz * 1e+6)), Zy_text.Length));
                    }
                    var fk_text = fk[e].Value.ToString("F"); fk_text = fk_text.Substring(0, Math.Min(5, fk_text.Length)); var fk2_text = "";
                    var ft_text = ft[e].Value.ToString("F"); ft_text = ft_text.Substring(0, Math.Min(5, ft_text.Length)); var ft2_text = "";
                    var fbi_text = fbi[e].Value.ToString("F"); fbi_text = fbi_text.Substring(0, Math.Min(5, fbi_text.Length)); var fbi2_text = "";
                    var fsi_text = fsi[e].Value.ToString("F"); fsi_text = fsi_text.Substring(0, Math.Min(5, fsi_text.Length)); var fsi2_text = "";
                    var fbo_text = fbo[e].Value.ToString("F"); fbo_text = fbo_text.Substring(0, Math.Min(5, fbo_text.Length)); var fbo2_text = "";
                    var fso_text = fso[e].Value.ToString("F"); fso_text = fso_text.Substring(0, Math.Min(5, fso_text.Length)); var fso2_text = "";
                    if (_fk.Branches.Count == 2) { fk2_text = fk2[e].Value.ToString("F"); fk2_text = fk2_text.Substring(0, Math.Min(5, fk2_text.Length)); }
                    if (_ft.Branches.Count == 2) { ft2_text = ft2[e].Value.ToString("F"); ft2_text = ft2_text.Substring(0, Math.Min(5, ft2_text.Length)); }
                    if (_fbi.Branches.Count == 2) { fbi2_text = fbi2[e].Value.ToString("F"); fbi2_text = fbi2_text.Substring(0, Math.Min(5, fbi2_text.Length)); }
                    if (_fsi.Branches.Count == 2) { fsi2_text = fsi2[e].Value.ToString("F"); fsi2_text = fsi2_text.Substring(0, Math.Min(5, fsi2_text.Length)); }
                    if (_fbo.Branches.Count == 2) { fbo2_text = fbo2[e].Value.ToString("F"); fbo2_text = fbo2_text.Substring(0, Math.Min(5, fbo2_text.Length)); }
                    if (_fso.Branches.Count == 2) { fso2_text = fso2[e].Value.ToString("F"); fso2_text = fso2_text.Substring(0, Math.Min(5, fso2_text.Length)); }
                    var sec_f = _sec_f.get_Branch(new GH_Path(new int[] { 0, e }));
                    var Ni_text = double.Parse(sec_f[0].ToString()).ToString("F"); Ni_text = Ni_text.Substring(0, Math.Min(4, Ni_text.Length));
                    var Qyi_text = Math.Abs(double.Parse(sec_f[1].ToString())).ToString("F"); Qyi_text = Qyi_text.Substring(0, Math.Min(4, Qyi_text.Length));
                    var Qzi_text = Math.Abs(double.Parse(sec_f[2].ToString())).ToString("F"); Qzi_text = Qzi_text.Substring(0, Math.Min(4, Qzi_text.Length));
                    var Myi_text = Math.Abs(double.Parse(sec_f[4].ToString())).ToString("F"); Myi_text = Myi_text.Substring(0, Math.Min(4, Myi_text.Length));
                    var Mzi_text = Math.Abs(double.Parse(sec_f[5].ToString())).ToString("F"); Mzi_text = Mzi_text.Substring(0, Math.Min(4, Mzi_text.Length));
                    var Nj_text = (-double.Parse(sec_f[6].ToString())).ToString("F"); Nj_text = Nj_text.Substring(0, Math.Min(4, Nj_text.Length));
                    var Qyj_text = Math.Abs(double.Parse(sec_f[7].ToString())).ToString("F"); Qyj_text = Qyj_text.Substring(0, Math.Min(4, Qyj_text.Length));
                    var Qzj_text = Math.Abs(double.Parse(sec_f[8].ToString())).ToString("F"); Qzj_text = Qzj_text.Substring(0, Math.Min(4, Qzj_text.Length));
                    var Myj_text = Math.Abs(double.Parse(sec_f[10].ToString())).ToString("F"); Myj_text = Myj_text.Substring(0, Math.Min(4, Myj_text.Length));
                    var Mzj_text = Math.Abs(double.Parse(sec_f[11].ToString())).ToString("F"); Mzj_text = Mzj_text.Substring(0, Math.Min(4, Mzj_text.Length));
                    var Nc_text = (double.Parse(sec_f[12].ToString())).ToString("F"); Nc_text = Nc_text.Substring(0, Math.Min(4, Nc_text.Length));
                    var Qyc_text = Math.Abs(double.Parse(sec_f[13].ToString())).ToString("F"); Qyc_text = Qyc_text.Substring(0, Math.Min(4, Qyc_text.Length));
                    var Qzc_text = Math.Abs(double.Parse(sec_f[14].ToString())).ToString("F"); Qzc_text = Qzc_text.Substring(0, Math.Min(4, Qzc_text.Length));
                    var Myc_text = Math.Abs(double.Parse(sec_f[16].ToString())).ToString("F"); Myc_text = Myc_text.Substring(0, Math.Min(4, Myc_text.Length));
                    var Mzc_text = Math.Abs(double.Parse(sec_f[17].ToString())).ToString("F"); Mzc_text = Mzc_text.Substring(0, Math.Min(4, Mzc_text.Length));
                    var klist = _kentei.get_Branch(new GH_Path(new int[] { 0, e }));
                    var Mki_color = new List<XSolidBrush>(); var Mkj_color = new List<XSolidBrush>(); var Mkc_color = new List<XSolidBrush>();
                    var Qki_color = new List<XSolidBrush>(); var Qkj_color = new List<XSolidBrush>(); var Qkc_color = new List<XSolidBrush>();
                    var Mki = (Math.Max(double.Parse(klist[3].ToString()), double.Parse(klist[4].ToString())) + double.Parse(klist[0].ToString()));
                    var Mki_text = Mki.ToString("F").Substring(0, 4); Mki_color.Add(new XSolidBrush(RGB((1 - Math.Min(Mki, 1.0)) * 1.9 / 3.0, 1, 0.5)));
                    var Mkj = (Math.Max(double.Parse(klist[8].ToString()), double.Parse(klist[9].ToString())) + double.Parse(klist[5].ToString()));
                    var Mkj_text = Mkj.ToString("F").Substring(0, 4); Mkj_color.Add(new XSolidBrush(RGB((1 - Math.Min(Mkj, 1.0)) * 1.9 / 3.0, 1, 0.5)));
                    var Mkc = (Math.Max(double.Parse(klist[13].ToString()), double.Parse(klist[14].ToString())) + double.Parse(klist[10].ToString()));
                    var Mkc_text = Mkc.ToString("F").Substring(0, 4); Mkc_color.Add(new XSolidBrush(RGB((1 - Math.Min(Mkc, 1.0)) * 1.9 / 3.0, 1, 0.5)));
                    var Qki = Math.Max(double.Parse(klist[1].ToString()), double.Parse(klist[2].ToString()));
                    var Qki_text = Qki.ToString("F").Substring(0, 4); Qki_color.Add(new XSolidBrush(RGB((1 - Math.Min(Qki, 1.0)) * 1.9 / 3.0, 1, 0.5)));
                    var Qkj = Math.Max(double.Parse(klist[6].ToString()), double.Parse(klist[7].ToString()));
                    var Qkj_text = Qkj.ToString("F").Substring(0, 4); Qkj_color.Add(new XSolidBrush(RGB((1 - Math.Min(Qkj, 1.0)) * 1.9 / 3.0, 1, 0.5)));
                    var Qkc = Math.Max(double.Parse(klist[11].ToString()), double.Parse(klist[12].ToString()));
                    var Qkc_text = Qkc.ToString("F").Substring(0, 4); Qkc_color.Add(new XSolidBrush(RGB((1 - Math.Min(Qkc, 1.0)) * 1.9 / 3.0, 1, 0.5)));
                    var OKi_text = "O.K."; var OKj_text = "O.K."; var OKc_text = "O.K.";
                    if (Mki > 1 || Qki > 1) { OKi_text = "N.G."; }
                    if (Mkj > 1 || Qkj > 1) { OKc_text = "N.G."; }
                    if (Mkc > 1 || Qkc > 1) { OKj_text = "N.G."; }
                    var values = new List<List<string>>();
                    values.Add(new List<string> { ele_text }); values.Add(new List<string> { name_text });
                    if (burnB.Count != 0 && burnD.Count != 0)
                    {
                        values.Add(new List<string> { ((int)(burnB[e] * 1000)).ToString() });
                        values.Add(new List<string> { ((int)(burnD[e] * 1000)).ToString() });
                    }
                    values.Add(new List<string> { lambda_text }); values.Add(new List<string> { A_text }); values.Add(new List<string> { alpha_text });
                    values.Add(new List<string> { Zy_text }); values.Add(new List<string> { Zz_text }); values.Add(new List<string> { "長期", "", "短期" });
                    if (burnB.Count == 0 && burnD.Count == 0) { values.Add(new List<string> { fk_text, "", fk2_text }); }
                    else { values.Add(new List<string> { "", "", fk_text }); }
                    values.Add(new List<string> { ft_text, "", ft2_text }); values.Add(new List<string> { fbi_text, "", fbi2_text }); values.Add(new List<string> { fsi_text, "", fsi2_text }); values.Add(new List<string> { fbo_text, "", fbo2_text }); values.Add(new List<string> { fso_text, "", fso2_text });
                    values.Add(new List<string> { "i端", "中央", "j端" }); values.Add(new List<string> { ni_text, "", nj_text });
                    if (burnB.Count == 0 && burnD.Count == 0) { values.Add(new List<string> { "長期検討" }); }
                    else { values.Add(new List<string> { "短期検討" }); }
                    values.Add(new List<string> { Ni_text, Nc_text, Nj_text }); values.Add(new List<string> { Myi_text, Myc_text, Myj_text }); values.Add(new List<string> { Mzi_text, Mzc_text, Mzj_text });
                    values.Add(new List<string> { Qyi_text, Qyc_text, Qyj_text }); values.Add(new List<string> { Qzi_text, Qzc_text, Qzj_text });
                    values.Add(new List<string> { Mki_text, Mkc_text, Mkj_text }); values.Add(new List<string> { Qki_text, Qkc_text, Qkj_text });
                    values.Add(new List<string> { OKi_text, OKc_text, OKj_text });
                    int N = _sec_f.Branches.Count / ij.Count;
                    if (N == 5)
                    {
                        var text = new List<string> { "短期(L+X)検討", "短期(L+Y)検討", "短期(L-X)検討", "短期(L-Y)検討" };
                        for (int i = 0; i < 4; i++)
                        {
                            values.Add(new List<string> { text[i] });
                            var sec_f1 = _sec_f.get_Branch(new GH_Path(new int[] { i + 1, e }));
                            Ni_text = (double.Parse(sec_f[0].ToString()) + double.Parse(sec_f1[0].ToString())).ToString("F"); Ni_text = Ni_text.Substring(0, Math.Min(4, Ni_text.Length));
                            Qyi_text = Math.Abs(double.Parse(sec_f[1].ToString()) + double.Parse(sec_f1[1].ToString())).ToString("F"); Qyi_text = Qyi_text.Substring(0, Math.Min(4, Qyi_text.Length));
                            Qzi_text = Math.Abs(double.Parse(sec_f[2].ToString()) + double.Parse(sec_f1[2].ToString())).ToString("F"); Qzi_text = Qzi_text.Substring(0, Math.Min(4, Qzi_text.Length));
                            Myi_text = Math.Abs(double.Parse(sec_f[4].ToString()) + double.Parse(sec_f1[4].ToString())).ToString("F"); Myi_text = Myi_text.Substring(0, Math.Min(4, Myi_text.Length));
                            Mzi_text = Math.Abs(double.Parse(sec_f[5].ToString()) + double.Parse(sec_f1[5].ToString())).ToString("F"); Mzi_text = Mzi_text.Substring(0, Math.Min(4, Mzi_text.Length));
                            Nj_text = (-double.Parse(sec_f[6].ToString()) - double.Parse(sec_f1[6].ToString())).ToString("F"); Nj_text = Nj_text.Substring(0, Math.Min(4, Nj_text.Length));
                            Qyj_text = Math.Abs(double.Parse(sec_f[7].ToString()) + double.Parse(sec_f1[7].ToString())).ToString("F"); Qyj_text = Qyj_text.Substring(0, Math.Min(4, Qyj_text.Length));
                            Qzj_text = Math.Abs(double.Parse(sec_f[8].ToString()) + double.Parse(sec_f1[8].ToString())).ToString("F"); Qzj_text = Qzj_text.Substring(0, Math.Min(4, Qzj_text.Length));
                            Myj_text = Math.Abs(double.Parse(sec_f[10].ToString()) + double.Parse(sec_f1[10].ToString())).ToString("F"); Myj_text = Myj_text.Substring(0, Math.Min(4, Myj_text.Length));
                            Mzj_text = Math.Abs(double.Parse(sec_f[11].ToString()) + double.Parse(sec_f1[11].ToString())).ToString("F"); Mzj_text = Mzj_text.Substring(0, Math.Min(4, Mzj_text.Length));
                            Nc_text = (double.Parse(sec_f[12].ToString()) + double.Parse(sec_f1[12].ToString())).ToString("F"); Nc_text = Nc_text.Substring(0, Math.Min(4, Nc_text.Length));
                            Qyc_text = Math.Abs(double.Parse(sec_f[13].ToString()) + double.Parse(sec_f1[13].ToString())).ToString("F"); Qyc_text = Qyc_text.Substring(0, Math.Min(4, Qyc_text.Length));
                            Qzc_text = Math.Abs(double.Parse(sec_f[14].ToString()) + double.Parse(sec_f1[14].ToString())).ToString("F"); Qzc_text = Qzc_text.Substring(0, Math.Min(4, Qzc_text.Length));
                            Myc_text = Math.Abs(double.Parse(sec_f[16].ToString()) + double.Parse(sec_f1[16].ToString())).ToString("F"); Myc_text = Myc_text.Substring(0, Math.Min(4, Myc_text.Length));
                            Mzc_text = Math.Abs(double.Parse(sec_f[17].ToString()) + double.Parse(sec_f1[17].ToString())).ToString("F"); Mzc_text = Mzc_text.Substring(0, Math.Min(4, Mzc_text.Length));
                            klist = _kentei.get_Branch(new GH_Path(new int[] { i + 1, e }));
                            Mki = (Math.Max(double.Parse(klist[3].ToString()), double.Parse(klist[4].ToString())) + double.Parse(klist[0].ToString()));
                            Mki_text = Mki.ToString("F").Substring(0, 4); Mki_color.Add(new XSolidBrush(RGB((1 - Math.Min(Mki, 1.0)) * 1.9 / 3.0, 1, 0.5)));
                            Mkj = (Math.Max(double.Parse(klist[8].ToString()), double.Parse(klist[9].ToString())) + double.Parse(klist[5].ToString()));
                            Mkj_text = Mkj.ToString("F").Substring(0, 4); Mkj_color.Add(new XSolidBrush(RGB((1 - Math.Min(Mkj, 1.0)) * 1.9 / 3.0, 1, 0.5)));
                            Mkc = (Math.Max(double.Parse(klist[13].ToString()), double.Parse(klist[14].ToString())) + double.Parse(klist[10].ToString()));
                            Mkc_text = Mkc.ToString("F").Substring(0, 4); Mkc_color.Add(new XSolidBrush(RGB((1 - Math.Min(Mkc, 1.0)) * 1.9 / 3.0, 1, 0.5)));
                            Qki = Math.Max(double.Parse(klist[1].ToString()), double.Parse(klist[2].ToString()));
                            Qki_text = Qki.ToString("F").Substring(0, 4); Qki_color.Add(new XSolidBrush(RGB((1 - Math.Min(Qki, 1.0)) * 1.9 / 3.0, 1, 0.5)));
                            Qkj = Math.Max(double.Parse(klist[6].ToString()), double.Parse(klist[7].ToString()));
                            Qkj_text = Qkj.ToString("F").Substring(0, 4); Qkj_color.Add(new XSolidBrush(RGB((1 - Math.Min(Qkj, 1.0)) * 1.9 / 3.0, 1, 0.5)));
                            Qkc = Math.Max(double.Parse(klist[11].ToString()), double.Parse(klist[12].ToString()));
                            Qkc_text = Qkc.ToString("F").Substring(0, 4); Qkc_color.Add(new XSolidBrush(RGB((1 - Math.Min(Qkc, 1.0)) * 1.9 / 3.0, 1, 0.5)));
                            OKi_text = "O.K."; OKj_text = "O.K."; OKc_text = "O.K.";
                            if (Mki > 1 || Qki > 1) { OKi_text = "N.G."; }
                            if (Mkj > 1 || Qkj > 1) { OKc_text = "N.G."; }
                            if (Mkc > 1 || Qkc > 1) { OKj_text = "N.G."; }
                            values.Add(new List<string> { Ni_text, Nc_text, Nj_text }); values.Add(new List<string> { Myi_text, Myc_text, Myj_text }); values.Add(new List<string> { Mzi_text, Mzc_text, Mzj_text });
                            values.Add(new List<string> { Qyi_text, Qyc_text, Qyj_text }); values.Add(new List<string> { Qzi_text, Qzc_text, Qzj_text });
                            values.Add(new List<string> { Mki_text, Mkc_text, Mkj_text }); values.Add(new List<string> { Qki_text, Qkc_text, Qkj_text });
                            values.Add(new List<string> { OKi_text, OKc_text, OKj_text });
                        }
                    }


                    if (e % 6 == 0)
                    {
                        // 空白ページを作成。
                        page = document.AddPage();
                        // 描画するためにXGraphicsオブジェクトを取得。
                        gfx = XGraphics.FromPdfPage(page);
                        for (int i = 0; i < labels.Count; i++)//ラベル列**************************************************************************
                        {
                            gfx.DrawLine(pen, offset_x, offset_y + pitchy * i, offset_x + label_width, offset_y + pitchy * i);//横線
                            gfx.DrawLine(pen, offset_x + label_width, offset_y + pitchy * i, offset_x + label_width, offset_y + pitchy * (i + 1));//縦線
                            gfx.DrawString(labels[i], font, XBrushes.Black, new XRect(offset_x, offset_y + pitchy * i, label_width, offset_y + pitchy * (i + 1)), XStringFormats.TopCenter);
                            if (i == labels.Count - 1)
                            {
                                i += 1;
                                gfx.DrawLine(pen, offset_x, offset_y + pitchy * i, offset_x + label_width, offset_y + pitchy * i);//横線
                            }
                        }//***********************************************************************************************************************
                    }
                    for (int i = 0; i < values.Count; i++)
                    {
                        var j = e % 6;
                        gfx.DrawLine(pen, offset_x + label_width + text_width * 3 * j, offset_y + pitchy * i, offset_x + label_width + text_width * 3 * (j + 1), offset_y + pitchy * i);//横線
                        gfx.DrawLine(pen, offset_x + label_width + text_width * 3 * j, offset_y + pitchy * i, offset_x + label_width + text_width * 3 * j, offset_y + pitchy * (i + 1));//縦線
                        if (values[i].Count == 1)
                        {
                            gfx.DrawString(values[i][0], font, XBrushes.Black, new XRect(offset_x + label_width + text_width * 3 * j, offset_y + pitchy * i, text_width * 3, offset_y + pitchy * (i + 1)), XStringFormats.TopCenter);
                        }
                        else
                        {
                            var color1 = XBrushes.Black; var color2 = XBrushes.Black; var color3 = XBrushes.Black; var f = font;
                            if (burnB.Count == 0 && burnD.Count == 0)
                            {
                                if (i == 22) { color1 = Mki_color[0]; color2 = Mkc_color[0]; color3 = Mkj_color[0]; f = fontbold; }
                                else if (i == 23) { color1 = Qki_color[0]; color2 = Qkc_color[0]; color3 = Qkj_color[0]; f = fontbold; }
                                else if (i == 31) { color1 = Mki_color[1]; color2 = Mkc_color[1]; color3 = Mkj_color[1]; f = fontbold; }
                                else if (i == 32) { color1 = Qki_color[1]; color2 = Qkc_color[1]; color3 = Qkj_color[1]; f = fontbold; }
                                else if (i == 40) { color1 = Mki_color[2]; color2 = Mkc_color[2]; color3 = Mkj_color[2]; f = fontbold; }
                                else if (i == 41) { color1 = Qki_color[2]; color2 = Qkc_color[2]; color3 = Qkj_color[2]; f = fontbold; }
                                else if (i == 49) { color1 = Mki_color[3]; color2 = Mkc_color[3]; color3 = Mkj_color[3]; f = fontbold; }
                                else if (i == 50) { color1 = Qki_color[3]; color2 = Qkc_color[3]; color3 = Qkj_color[3]; f = fontbold; }
                                else if (i == 58) { color1 = Mki_color[4]; color2 = Mkc_color[4]; color3 = Mkj_color[4]; f = fontbold; }
                                else if (i == 59) { color1 = Qki_color[4]; color2 = Qkc_color[4]; color3 = Qkj_color[4]; f = fontbold; }
                            }
                            else
                            {
                                if (i == 24) { color1 = Mki_color[0]; color2 = Mkc_color[0]; color3 = Mkj_color[0]; f = fontbold; }
                                else if (i == 25) { color1 = Qki_color[0]; color2 = Qkc_color[0]; color3 = Qkj_color[0]; f = fontbold; }
                            }
                            gfx.DrawString(values[i][0], f, color1, new XRect(offset_x + label_width + text_width * 3 * j, offset_y + pitchy * i, text_width, offset_y + pitchy * (i + 1)), XStringFormats.TopCenter);
                            gfx.DrawString(values[i][1], f, color2, new XRect(offset_x + label_width + text_width * 3 * j + text_width, offset_y + pitchy * i, text_width, offset_y + pitchy * (i + 1)), XStringFormats.TopCenter);
                            gfx.DrawString(values[i][2], f, color3, new XRect(offset_x + label_width + text_width * 3 * j + text_width * 2, offset_y + pitchy * i, text_width, offset_y + pitchy * (i + 1)), XStringFormats.TopCenter);
                        }
                        if (i == values.Count - 1)
                        {
                            i += 1;
                            gfx.DrawLine(pen, offset_x + label_width + text_width * 3 * j, offset_y + pitchy * i, offset_x + label_width + text_width * 3 * (j + 1), offset_y + pitchy * i);//横線
                        }
                    }
                }
                var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                // ドキュメントを保存。
                var filename = dir + "/" + pdfname + ".pdf";
                document.Save(filename);
                // ビューアを起動。
                Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return OpenSeesUtility.Properties.Resources.cltcheckpdf;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("48f43466-1c0d-4322-816a-c7aa473350a1"); }
        }///ここからGUIの作成*****************************************************************************************
        public class CustomGUI : GH_ComponentAttributes
        {
            public CustomGUI(GH_Component owner) : base(owner)
            {
            }
            private Rectangle radio_rec1;
            private Rectangle radio_rec1_1; private Rectangle text_rec1_1;
            protected override void Layout()
            {
                base.Layout();
                Rectangle global_rec = GH_Convert.ToRectangle(Bounds);
                int height = 15; int radi1 = 7; int radi2 = 4;
                int pitchx = 8; int textheight = 20;
                int width = global_rec.Width;

                radio_rec1 = global_rec; radio_rec1.Y = radio_rec1.Bottom;
                radio_rec1.Height = height;
                global_rec.Height += height;

                radio_rec1_1 = radio_rec1;
                radio_rec1_1.X += 5; radio_rec1_1.Y += 5;
                radio_rec1_1.Height = radi1; radio_rec1_1.Width = radi1;

                text_rec1_1 = radio_rec1_1;
                text_rec1_1.X += pitchx; text_rec1_1.Y -= radi2;
                text_rec1_1.Height = textheight; text_rec1_1.Width = width;

                Bounds = global_rec;
            }
            Brush c1 = Brushes.White;
            protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
            {
                base.Render(canvas, graphics, channel);
                if (channel == GH_CanvasChannel.Objects)
                {
                    GH_Capsule radio1 = GH_Capsule.CreateCapsule(radio_rec1, GH_Palette.White, 2, 0);
                    radio1.Render(graphics, Selected, Owner.Locked, false); radio1.Dispose();

                    GH_Capsule radio1_1 = GH_Capsule.CreateCapsule(radio_rec1_1, GH_Palette.Black, 5, 5);
                    radio1_1.Render(graphics, Selected, Owner.Locked, false); radio1_1.Dispose();
                    graphics.FillEllipse(c1, radio_rec1_1);
                    graphics.DrawString("PDF OUTPUT", GH_FontServer.Standard, Brushes.Black, text_rec1_1);
                }

            }
            public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    RectangleF rec1 = radio_rec1_1;
                    if (rec1.Contains(e.CanvasLocation))
                    {
                        if (c1 == Brushes.White) { c1 = Brushes.Black; SetButton("1", 1); }
                        else { c1 = Brushes.White; SetButton("1", 0); }
                        Owner.ExpireSolution(true);
                        return GH_ObjectResponse.Handled;
                    }
                }
                return base.RespondToMouseDown(sender, e);
            }
        }
    }
}