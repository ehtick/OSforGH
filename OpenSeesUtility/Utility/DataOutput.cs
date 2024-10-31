﻿using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.IO;
using System.Linq;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp;
using System.Drawing;
using System.Windows.Forms;
///for GUI*********************************
using Grasshopper.Kernel.Attributes;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
///****************************************
using System.Diagnostics;

namespace OpenSeesUtility
{
    public class DataOutput : GH_Component
    {
        public static int on_off = 0; public static double fontsize = 9;
        public static PdfCreate.JapaneseFontResolver fontresolver = new PdfCreate.JapaneseFontResolver();
        public static XGraphics gfx;
        public static void SetButton(string s, int i)
        {
            if (s == "1")
            {
                on_off = i;
            }
        }
        public DataOutput()
          : base("DataOutput", "DataOutput",
              "Echo data output",
              "OpenSees", "Utility")
        {
        }
        public override void CreateAttributes()
        {
            m_attributes = new CustomGUI(this);
        }
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("nodal_coordinates", "R", "[[x1,y1,z1],...](DataTree)", GH_ParamAccess.tree);///
            pManager.AddNumberParameter("element_node_relationship", "IJ", "[[No.i,No.j,material No.,section No.,angle],...](DataTree)", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("element_node_relationship(shell)", "IJKL", "[[No.i,No.j,No.k,No.l(if triangle:-1),material No.,thickness],...](DataTree)", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("boundary_condition", "B", "[[node No.,X,Y,Z,MX,MY,MZ],...](DataTree) 0-1 variable", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("joint condition", "joint", "[[Ele. No., 0 or 1(means i or j), kx, ky, kz, rx, ry, rz(spring value)],...](DataTree)", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("spring element", "spring", "[[No.i, No.j, kxt, ktc, kyt, kyc, kzt, kzc, rx, ry, rz(spring value)],...](DataTree)", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("spring_allowable_f", "spring_a", "[[Nta,Nca,Qyta,Qyca,Qzta,Qzca,Mxa,Mya,Mza],...](DataTree)", GH_ParamAccess.tree,-9999);///
            pManager.AddTextParameter("secname", "secname", "[secname0,secname1,...]", GH_ParamAccess.list,"-9999");///
            pManager.AddNumberParameter("p_load1", "p_load1", "[[No,Fx,Fy,Fz,Mx,My,Mz],...]", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("p_load2", "p_load2", "[[No,Fx,Fy,Fz,Mx,My,Mz],...]", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("p_load3", "p_load3", "[[No,Fx,Fy,Fz,Mx,My,Mz],...]", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("e_load1", "e_load1", "[[No.,Wx,Wy,Wz],...]", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("e_load2", "e_load2", "[[No.,Wx,Wy,Wz],...]", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("e_load3", "e_load3", "[[No.,Wx,Wy,Wz],...]", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("sf_load1", "sf_load1", "[[No.i,No.j,No.k,No.l,Sz],...]", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("sf_load2", "sf_load2", "[[No.i,No.j,No.k,No.l,Sz],...]", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("sf_load3", "sf_load3", "[[No.i,No.j,No.k,No.l,Sz],...]", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("w_load1", "w_load1", "[[No.i,No.j,No.k,No.l,Wz],...]", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("w_load2", "w_load2", "[[No.i,No.j,No.k,No.l,Wz],...]", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("w_load3", "w_load3", "[[No.i,No.j,No.k,No.l,Wz],...]", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("disp", "D(R)", "[[dx,dy,dz,theta_x,theta_y,theta_z],...](DataTree)", GH_ParamAccess.tree, -9999);
            pManager.AddNumberParameter("reaction_force", "reac_f", "[[Node No.,Px,Py,Pz,Mx,My,Mz],...](DataTree)", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("sectional_force1", "sec_f1", "[[element No.,Pxi,Pyi,Pzi,Mxi,Myi,Mzi,Pxj,Pyj,Pzj,Mxj,Myj,Mzj,Pxc,Pyc,Pzc,Mxc,Myc,Mzc],...](DataTree)", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("sectional_force2", "sec_f2", "[[element No.,Pxi,Pyi,Pzi,Mxi,Myi,Mzi,Pxj,Pyj,Pzj,Mxj,Myj,Mzj,Pxc,Pyc,Pzc,Mxc,Myc,Mzc],...](DataTree)", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("sectional_force3", "sec_f3", "[[element No.,Pxi,Pyi,Pzi,Mxi,Myi,Mzi,Pxj,Pyj,Pzj,Mxj,Myj,Mzj,Pxc,Pyc,Pzc,Mxc,Myc,Mzc],...](DataTree)", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("sectional_force4", "sec_f4", "[[element No.,Pxi,Pyi,Pzi,Mxi,Myi,Mzi,Pxj,Pyj,Pzj,Mxj,Myj,Mzj,Pxc,Pyc,Pzc,Mxc,Myc,Mzc],...](DataTree)", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("spring_f1", "spring_f1", "[[N,Qy,Qz,Mx,My,Mz],...](DataTree)", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("spring_f2", "spring_f2", "[[N,Qy,Qz,Mx,My,Mz],...](DataTree)", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("spring_f3", "spring_f3", "[[N,Qy,Qz,Mx,My,Mz],...](DataTree)", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("spring_f4", "spring_f4", "[[N,Qy,Qz,Mx,My,Mz],...](DataTree)", GH_ParamAccess.tree, -9999);///
            pManager.AddNumberParameter("spring_f5", "spring_f5", "[[N,Qy,Qz,Mx,My,Mz],...](DataTree)", GH_ParamAccess.tree, -9999);///
            pManager.AddTextParameter("outputname", "outputname", "output file name", GH_ParamAccess.item, "DataOutput");///
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
            var pdfname = "DataOutput"; DA.GetData("outputname", ref pdfname);
            DA.GetDataTree("nodal_coordinates", out GH_Structure<GH_Number> _r); var r = _r.Branches;
            DA.GetDataTree("element_node_relationship", out GH_Structure<GH_Number> _ij); var ij = _ij.Branches;
            DA.GetDataTree("element_node_relationship(shell)", out GH_Structure<GH_Number> _ijkl); var ijkl = _ijkl.Branches;
            DA.GetDataTree("boundary_condition", out GH_Structure<GH_Number> _B); var B = _B.Branches;
            DA.GetDataTree("joint condition", out GH_Structure<GH_Number> _joint); var joint = _joint.Branches;
            DA.GetDataTree("p_load1", out GH_Structure<GH_Number> _p_load1); var p_load1 = _p_load1.Branches;
            DA.GetDataTree("p_load2", out GH_Structure<GH_Number> _p_load2); var p_load2 = _p_load2.Branches;
            DA.GetDataTree("p_load3", out GH_Structure<GH_Number> _p_load3); var p_load3 = _p_load3.Branches;
            DA.GetDataTree("e_load1", out GH_Structure<GH_Number> _e_load); var e_load = _e_load.Branches;
            DA.GetDataTree("e_load2", out GH_Structure<GH_Number> _e_load2); var e_load2 = _e_load2.Branches;
            DA.GetDataTree("e_load3", out GH_Structure<GH_Number> _e_load3); var e_load3 = _e_load3.Branches;
            DA.GetDataTree("sf_load1", out GH_Structure<GH_Number> _sf_load); var sf_load = _sf_load.Branches;
            DA.GetDataTree("sf_load2", out GH_Structure<GH_Number> _sf_load2); var sf_load2 = _sf_load2.Branches;
            DA.GetDataTree("sf_load3", out GH_Structure<GH_Number> _sf_load3); var sf_load3 = _sf_load3.Branches;
            DA.GetDataTree("w_load1", out GH_Structure<GH_Number> _w_load); var w_load = _w_load.Branches;
            DA.GetDataTree("w_load2", out GH_Structure<GH_Number> _w_load2); var w_load2 = _w_load2.Branches;
            DA.GetDataTree("w_load3", out GH_Structure<GH_Number> _w_load3); var w_load3 = _w_load3.Branches;
            DA.GetDataTree("spring element", out GH_Structure<GH_Number> _spring); var spring = _spring.Branches;
            DA.GetDataTree("spring_allowable_f", out GH_Structure<GH_Number> _spring_a); var spring_a = _spring_a.Branches;
            DA.GetDataTree("disp", out GH_Structure<GH_Number> _d); var d = _d.Branches;
            DA.GetDataTree("reaction_force", out GH_Structure<GH_Number> _reac_f); var reac_f = _reac_f.Branches;
            DA.GetDataTree("sectional_force1", out GH_Structure<GH_Number> _sec_f1); var sec_f1 = _sec_f1.Branches;
            DA.GetDataTree("sectional_force2", out GH_Structure<GH_Number> _sec_f2); var sec_f2 = _sec_f2.Branches;
            DA.GetDataTree("sectional_force3", out GH_Structure<GH_Number> _sec_f3); var sec_f3 = _sec_f3.Branches;
            DA.GetDataTree("sectional_force4", out GH_Structure<GH_Number> _sec_f4); var sec_f4 = _sec_f4.Branches;
            DA.GetDataTree("spring_f1", out GH_Structure<GH_Number> _spring_f1); var spring_f1 = _spring_f1.Branches;
            DA.GetDataTree("spring_f2", out GH_Structure<GH_Number> _spring_f2); var spring_f2 = _spring_f2.Branches;
            DA.GetDataTree("spring_f3", out GH_Structure<GH_Number> _spring_f3); var spring_f3 = _spring_f3.Branches;
            DA.GetDataTree("spring_f4", out GH_Structure<GH_Number> _spring_f4); var spring_f4 = _spring_f4.Branches;
            DA.GetDataTree("spring_f5", out GH_Structure<GH_Number> _spring_f5); var spring_f5 = _spring_f5.Branches;
            List<string> secname = new List<string>(); DA.GetDataList("secname", secname);
            if (on_off == 1)
            {
                // フォントリゾルバーのグローバル登録
                if (PdfCreate.JapaneseFontResolver.fontset == 0) { PdfSharp.Fonts.GlobalFontSettings.FontResolver = fontresolver; PdfCreate.JapaneseFontResolver.fontset = 1; }
                // フォントを作成。
                XFont font = new XFont("Gen Shin Gothic", fontsize, XFontStyle.Regular);
                if (r[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 40; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = 170;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 0;
                    var labels = new List<string> { "No.", "X[m]", "Y[m]", "Z[m]" };
                    for (int i = 0; i < r.Count; i++)
                    {
                        if (i % lines == 0)
                        {
                            count = 0;
                            if (i % (lines * 3) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy, offset_x + label_width * (j + 1) + slide, offset_y + pitchy);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { i.ToString(), r[i][0].Value.ToString("F"), r[i][1].Value.ToString("F"), r[i][2].Value.ToString("F") };
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_r.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (ij[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 32; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 8 + 5;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 0;
                    var pin = new List<string>();
                    for (int i = 0; i < ij.Count; i++) { pin.Add("|---|"); }
                    if (joint[0][0].Value != -9999)
                    {
                        for (int j = 0; j < joint.Count; j++)
                        {
                            int n = (int)joint[j][0].Value; int jj = (int)joint[j][1].Value;
                            if (jj == 0) { pin[n] = "o---|"; }
                            else if (jj == 1) { pin[n] = "|---o"; }
                            else if (jj == 2) { pin[n] = "o---o"; }
                        }
                    }
                    for (int i = 0; i < ij.Count; i++)
                    {
                        var labels = new List<string> { "No.", "i", "j", "mat", "sec", "angle", "joint","L[m]" };
                        int ni = (int)ij[i][0].Value; int nj = (int)ij[i][1].Value;
                        var l = Math.Sqrt(Math.Pow(r[ni][0].Value - r[nj][0].Value,2)+ Math.Pow(r[ni][1].Value - r[nj][1].Value, 2)+ Math.Pow(r[ni][2].Value - r[nj][2].Value, 2));
                        if (i % lines == 0)
                        {
                            count = 0;
                            if (i % (lines * 2) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy, offset_x + label_width * (j + 1) + slide, offset_y + pitchy);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { i.ToString(), ij[i][0].ToString(), ij[i][1].Value.ToString(), ij[i][2].Value.ToString(), ij[i][3].Value.ToString(), ij[i][4].Value.ToString("F"), pin[i],l.ToString("F") };
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_ij.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (B[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 30; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 7 + 5;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 0;
                    for (int i = 0; i < B.Count; i++)
                    {
                        var labels = new List<string> { "No.", "fx", "fy", "fz", "rx", "ry", "rz" };
                        if (i % lines == 0)
                        {
                            count = 0;
                            if (i % (lines * 2) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy, offset_x + label_width * (j + 1) + slide, offset_y + pitchy);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { B[i][0].ToString(), B[i][1].ToString(), B[i][2].ToString(), B[i][3].ToString(), B[i][4].ToString(), B[i][5].ToString(), B[i][6].ToString() };
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_bnds.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (p_load1[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 40; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 7 + 1;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 1;
                    var labels = new List<string> { "No.", "Fx", "Fy", "Fz", "Mx", "My", "Mz" };
                    var labels2 = new List<string> { "", "[kN]", "[kN]", "[kN]", "[kNm]", "[kNm]", "[kNm]" };
                    var Fx = 0.0; var Fy = 0.0; var Fz = 0.0; var Mx = 0.0; var My = 0.0; var Mz = 0.0;
                    for (int i = 0; i < p_load1.Count; i++)
                    {
                        var No = (int)p_load1[i][0].Value;
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 2) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy * 2), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { No.ToString(), p_load1[i][1].Value.ToString("F"), p_load1[i][2].Value.ToString("F"), p_load1[i][3].Value.ToString("F"), p_load1[i][4].Value.ToString("F"), p_load1[i][5].Value.ToString("F"), p_load1[i][6].Value.ToString("F") };
                        Fx += p_load1[i][1].Value; Fy += p_load1[i][2].Value; Fz += p_load1[i][3].Value; Mx += p_load1[i][4].Value; My += p_load1[i][5].Value; Mz += p_load1[i][6].Value;
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    count += 1;
                    var total_values = new List<string> { "sum", Fx.ToString("F"), Fy.ToString("F"), Fz.ToString("F"), Mx.ToString("F"), My.ToString("F"), Mz.ToString("F") };
                    for (int j = 0; j < total_values.Count; j++)
                    {
                        gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                        gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                        gfx.DrawString(total_values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                        if (j == labels.Count - 1)
                        {
                            j += 1;
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_p1.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (p_load2[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 40; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 7 + 1;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 1;
                    var labels = new List<string> { "No.", "Fx", "Fy", "Fz", "Mx", "My", "Mz" };
                    var labels2 = new List<string> { "", "[kN]", "[kN]", "[kN]", "[kNm]", "[kNm]", "[kNm]" };
                    var Fx = 0.0; var Fy = 0.0; var Fz = 0.0; var Mx = 0.0; var My = 0.0; var Mz = 0.0;
                    for (int i = 0; i < p_load2.Count; i++)
                    {
                        var No = (int)p_load2[i][0].Value;
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 2) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy * 2), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { No.ToString(), p_load2[i][1].Value.ToString("F"), p_load2[i][2].Value.ToString("F"), p_load2[i][3].Value.ToString("F"), p_load2[i][4].Value.ToString("F"), p_load2[i][5].Value.ToString("F"), p_load2[i][6].Value.ToString("F") };
                        Fx += p_load2[i][1].Value; Fy += p_load2[i][2].Value; Fz += p_load2[i][3].Value; Mx += p_load2[i][4].Value; My += p_load2[i][5].Value; Mz += p_load2[i][6].Value;
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    count += 1;
                    var total_values = new List<string> { "sum", Fx.ToString("F"), Fy.ToString("F"), Fz.ToString("F"), Mx.ToString("F"), My.ToString("F"), Mz.ToString("F") };
                    for (int j = 0; j < total_values.Count; j++)
                    {
                        gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                        gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                        gfx.DrawString(total_values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                        if (j == labels.Count - 1)
                        {
                            j += 1;
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_p2.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (p_load3[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 40; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 7 + 1;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 1;
                    var labels = new List<string> { "No.", "Fx", "Fy", "Fz", "Mx", "My", "Mz" };
                    var labels2 = new List<string> { "", "[kN]", "[kN]", "[kN]", "[kNm]", "[kNm]", "[kNm]" };
                    var Fx = 0.0; var Fy = 0.0; var Fz = 0.0; var Mx = 0.0; var My = 0.0; var Mz = 0.0;
                    for (int i = 0; i < p_load3.Count; i++)
                    {
                        var No = (int)p_load3[i][0].Value;
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 2) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy * 2), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { No.ToString(), p_load3[i][1].Value.ToString("F"), p_load3[i][2].Value.ToString("F"), p_load3[i][3].Value.ToString("F"), p_load3[i][4].Value.ToString("F"), p_load3[i][5].Value.ToString("F"), p_load3[i][6].Value.ToString("F") };
                        Fx += p_load3[i][1].Value; Fy += p_load3[i][2].Value; Fz += p_load3[i][3].Value; Mx += p_load3[i][4].Value; My += p_load3[i][5].Value; Mz += p_load3[i][6].Value;
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    count += 1;
                    var total_values = new List<string> { "sum", Fx.ToString("F"), Fy.ToString("F"), Fz.ToString("F"), Mx.ToString("F"), My.ToString("F"), Mz.ToString("F") };
                    for (int j = 0; j < total_values.Count; j++)
                    {
                        gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                        gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                        gfx.DrawString(total_values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                        if (j == labels.Count - 1)
                        {
                            j += 1;
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_p3.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (e_load[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 40; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 4 + 5;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 1;
                    var labels = new List<string> { "No.", "Wx", "Wy", "Wz" };
                    var labels2 = new List<string> { "", "[kN/m]", "[kN/m]", "[kN/m]" };
                    for (int i = 0; i < e_load.Count; i++)
                    {
                        var No = (int)e_load[i][0].Value;
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 3) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy * 2), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { No.ToString(), e_load[i][1].Value.ToString("F"), e_load[i][2].Value.ToString("F"), e_load[i][3].Value.ToString("F")};
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_e_load1.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (e_load2[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 40; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 4 + 5;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 1;
                    var labels = new List<string> { "No.", "Wx", "Wy", "Wz" };
                    var labels2 = new List<string> { "", "[kN/m]", "[kN/m]", "[kN/m]" };
                    for (int i = 0; i < e_load2.Count; i++)
                    {
                        var No = (int)e_load2[i][0].Value;
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 3) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy * 2), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { No.ToString(), e_load2[i][1].Value.ToString("F"), e_load2[i][2].Value.ToString("F"), e_load2[i][3].Value.ToString("F") };
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_e_load2.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (e_load3[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 40; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 4 + 5;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 1;
                    var labels = new List<string> { "No.", "Wx", "Wy", "Wz" };
                    var labels2 = new List<string> { "", "[kN/m]", "[kN/m]", "[kN/m]" };
                    for (int i = 0; i < e_load3.Count; i++)
                    {
                        var No = (int)e_load3[i][0].Value;
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 3) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy * 2), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { No.ToString(), e_load3[i][1].Value.ToString("F"), e_load3[i][2].Value.ToString("F"), e_load3[i][3].Value.ToString("F") };
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_e_load3.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (sf_load[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 40; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 5 + 5;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var labels = new List<string> { "No.", "No.", "No.", "No.","Sz" };
                    var labels2 = new List<string> { "i", "j", "k", "l", "[kN/m2]" };
                    if (sf_load[0].Count >= 10){ labels.Add("Sz2"); labels2.Add("[kN/m2]"); clear += label_width; }
                    var slide = -clear; var count = 1;
                    for (int i = 0; i < sf_load.Count; i++)
                    {
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 2) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy * 2), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1; var n4 = ((int)sf_load[i][3].Value).ToString();
                        if ((int)sf_load[i][3].Value == -1) { n4 = "--"; }
                        var values = new List<string> { ((int)sf_load[i][0].Value).ToString(), ((int)sf_load[i][1].Value).ToString(), ((int)sf_load[i][2].Value).ToString(), n4, sf_load[i][4].Value.ToString("F")};
                        if (sf_load[0].Count >= 10) { values.Add(sf_load[i][9].Value.ToString("F")); }
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_sf_load.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (sf_load2[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 40; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 5 + 5;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var labels = new List<string> { "No.", "No.", "No.", "No.", "Sz" };
                    var labels2 = new List<string> { "i", "j", "k", "l", "[kN/m2]" };
                    if (sf_load2[0].Count >= 10) { labels.Add("Sz2"); labels2.Add("[kN/m2]"); clear += label_width; }
                    var slide = -clear; var count = 1;
                    for (int i = 0; i < sf_load2.Count; i++)
                    {
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 2) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy * 2), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1; var n4 = ((int)sf_load2[i][3].Value).ToString();
                        if ((int)sf_load2[i][3].Value == -1) { n4 = "--"; }
                        var values = new List<string> { ((int)sf_load2[i][0].Value).ToString(), ((int)sf_load2[i][1].Value).ToString(), ((int)sf_load2[i][2].Value).ToString(), n4, sf_load2[i][4].Value.ToString("F") };
                        if (sf_load2[0].Count >= 10) { values.Add(sf_load2[i][9].Value.ToString("F")); }
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_sf_load2.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (sf_load3[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 40; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 5 + 5;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var labels = new List<string> { "No.", "No.", "No.", "No.", "Sz" };
                    var labels2 = new List<string> { "i", "j", "k", "l", "[kN/m2]" };
                    if (sf_load3[0].Count >= 10) { labels.Add("Sz2"); labels2.Add("[kN/m2]"); clear += label_width; }
                    var slide = -clear; var count = 1;
                    for (int i = 0; i < sf_load3.Count; i++)
                    {
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 2) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy * 2), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1; var n4 = ((int)sf_load3[i][3].Value).ToString();
                        if ((int)sf_load3[i][3].Value == -1) { n4 = "--"; }
                        var values = new List<string> { ((int)sf_load3[i][0].Value).ToString(), ((int)sf_load3[i][1].Value).ToString(), ((int)sf_load3[i][2].Value).ToString(), n4, sf_load3[i][4].Value.ToString("F") };
                        if (sf_load3[0].Count >= 10) { values.Add(sf_load3[i][9].Value.ToString("F")); }
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_sf_load3.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (w_load[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 40; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 5 + 5;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var labels = new List<string> { "No.", "No.", "No.", "No.", "Wz" };
                    var labels2 = new List<string> { "i", "j", "k", "l", "[kN/m2]" };
                    if (w_load[0].Count >= 10) { labels.Add("Wz2"); labels2.Add("[kN/m2]"); clear += label_width; }
                    var slide = -clear; var count = 1;
                    for (int i = 0; i < w_load.Count; i++)
                    {
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 2) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy * 2), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1; var n4 = ((int)w_load[i][3].Value).ToString();
                        if ((int)w_load[i][3].Value == -1) { n4 = "--"; }
                        var values = new List<string> { ((int)w_load[i][0].Value).ToString(), ((int)w_load[i][1].Value).ToString(), ((int)w_load[i][2].Value).ToString(), n4, w_load[i][4].Value.ToString("F") };
                        if (w_load[0].Count >= 10) { values.Add(w_load[i][9].Value.ToString("F")); }
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_w_load.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (w_load2[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 40; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 5 + 5;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var labels = new List<string> { "No.", "No.", "No.", "No.", "Wz" };
                    var labels2 = new List<string> { "i", "j", "k", "l", "[kN/m2]" };
                    if (w_load2[0].Count >= 10) { labels.Add("Wz2"); labels2.Add("[kN/m2]"); clear += label_width; }
                    var slide = -clear; var count = 1;
                    for (int i = 0; i < w_load2.Count; i++)
                    {
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 2) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy * 2), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1; var n4 = ((int)w_load2[i][3].Value).ToString();
                        if ((int)w_load2[i][3].Value == -1) { n4 = "--"; }
                        var values = new List<string> { ((int)w_load2[i][0].Value).ToString(), ((int)w_load2[i][1].Value).ToString(), ((int)w_load2[i][2].Value).ToString(), n4, w_load2[i][4].Value.ToString("F") };
                        if (w_load2[0].Count >= 10) { values.Add(w_load2[i][9].Value.ToString("F")); }
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_w_load2.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (w_load3[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 40; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 5 + 5;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var labels = new List<string> { "No.", "No.", "No.", "No.", "Wz" };
                    var labels2 = new List<string> { "i", "j", "k", "l", "[kN/m2]" };
                    if (w_load3[0].Count >= 10) { labels.Add("Wz2"); labels2.Add("[kN/m2]"); clear += label_width; }
                    var slide = -clear; var count = 1;
                    for (int i = 0; i < w_load3.Count; i++)
                    {
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 2) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy * 2), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1; var n4 = ((int)w_load3[i][3].Value).ToString();
                        if ((int)w_load3[i][3].Value == -1) { n4 = "--"; }
                        var values = new List<string> { ((int)w_load3[i][0].Value).ToString(), ((int)w_load3[i][1].Value).ToString(), ((int)w_load3[i][2].Value).ToString(), n4, w_load3[i][4].Value.ToString("F") };
                        if (w_load3[0].Count >= 10) { values.Add(w_load3[i][9].Value.ToString("F")); }
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_w_load3.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (spring[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 45; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 8 + 5;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 0;
                    for (int i = 0; i < spring.Count; i++)
                    {
                        var labels = new List<string> { "i", "j", "kxt", "kxc", "kyt", "kyc", "kzt", "kzc", "kmx", "kmy", "kmz" };
                        var labels2 = new List<string> { "", "", "[kN/m]", "[kN/m]", "[kN/m]", "[kN/m]", "[kN/m]", "[kN/m]", "[kNm/rad]", "[kNm/rad]", "[kNm/rad]" };
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                if (j <= 1)
                                {
                                    gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * 0.5, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                }
                                else
                                {
                                    gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                }
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string>();
                        for (int j = 0; j < spring[i].Count; j++)
                        {
                            var value = spring[i][j].Value;
                            if (value >= 999999) { values.Add("---"); }
                            else { values.Add(Math.Round(value,0).ToString("G")); }
                        }
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_spring.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (secname[0] != "-9999")
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 30; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 4 + 5;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 0;
                    var labels = new List<string> { "No.", "", "section name", "" };
                    for (int i = 0; i < secname.Count; i++)
                    {
                        if (i % lines == 0)
                        {
                            count = 0;
                            if (i % (lines * 4) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy, offset_x + label_width * (j + 1) + slide, offset_y + pitchy);//横線
                                if (j <= 1)
                                {
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy);
                                }//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { i.ToString(), "", secname[i],"" };
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            if (j <= 1)
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_sec.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (d[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 50; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 4;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 0;
                    var labels = new List<string> { "No.", "dx[mm]", "dy[mm]", "dz[mm]" };
                    var labels2 = new List<string> { "", "", "(L)", "" };
                    if (d[0].Count >= 18)
                    {
                        labels.Add("dx[mm]"); labels.Add("dy[mm]"); labels.Add("dz[mm]"); labels.Add("dx[mm]"); labels.Add("dy[mm]"); labels.Add("dz[mm]");
                        labels2.Add(""); labels2.Add("(X)"); labels2.Add(""); labels2.Add(""); labels2.Add("(Y)"); labels2.Add("");
                    }
                    for (int i = 0; i < r.Count; i++)
                    {
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 1) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy, offset_x + label_width * (j + 1) + slide, offset_y + pitchy);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                if (j <= 1 || j == 4 || j == 7) { gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy); }//縦線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { i.ToString(), (d[i][0].Value * 1000).ToString("F4"), (d[i][1].Value * 1000).ToString("F4"), (d[i][2].Value * 1000).ToString("F4") };
                        if (d[0].Count >= 18)
                        {
                            values.Add((d[i][6 + 0].Value * 1000).ToString("F4")); values.Add((d[i][6 + 1].Value * 1000).ToString("F4")); values.Add((d[i][6 + 2].Value * 1000).ToString("F4"));
                            values.Add((d[i][12 + 0].Value * 1000).ToString("F4")); values.Add((d[i][12 + 1].Value * 1000).ToString("F4")); values.Add((d[i][12 + 2].Value * 1000).ToString("F4"));
                        }
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_disp.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (reac_f[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 29; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 4;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 0;
                    var labels = new List<string> { "No.", "Fx", "Fy", "Fz", "Mx", "My", "Mz" };
                    var labels2 = new List<string> { "", "", "", "        (L)", "", "", "" };
                    var labels3 = new List<string> { "", "[kN]", "[kN]", "[kN]", "[kNm]", "[kNm]", "[kNm]" };
                    var FxL = 0.0; var FyL = 0.0; var FzL = 0.0; var MxL = 0.0; var MyL = 0.0; var MzL = 0.0;
                    var FxX = 0.0; var FyX = 0.0; var FzX = 0.0; var MxX = 0.0; var MyX = 0.0; var MzX = 0.0;
                    var FxY = 0.0; var FyY = 0.0; var FzY = 0.0; var MxY = 0.0; var MyY = 0.0; var MzY = 0.0;
                    if (reac_f[0].Count >= 21)
                    {
                        labels.Add("Fx"); labels.Add("Fy"); labels.Add("Fz"); labels.Add("Mx"); labels.Add("My"); labels.Add("Mz"); labels.Add("Fx"); labels.Add("Fy"); labels.Add("Fz"); labels.Add("Mx"); labels.Add("My"); labels.Add("Mz");
                        labels2.Add(""); labels2.Add(""); labels2.Add("        (X)"); labels2.Add(""); labels2.Add(""); labels2.Add(""); labels2.Add(""); labels2.Add(""); labels2.Add("        (Y)"); labels2.Add(""); labels2.Add(""); labels2.Add("");
                        labels3.Add("[kN]"); labels3.Add("[kN]"); labels3.Add("[kN]"); labels3.Add("[kNm]"); labels3.Add("[kNm]"); labels3.Add("[kNm]"); labels3.Add("[kN]"); labels3.Add("[kN]"); labels3.Add("[kN]"); labels3.Add("[kNm]"); labels3.Add("[kNm]"); labels3.Add("[kNm]");
                    }
                    for (int i = 0; i < reac_f.Count; i++)
                    {
                        if (i % lines == 0)
                        {
                            count = 2;
                            if (i % (lines * 1) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy, offset_x + label_width * (j + 1) + slide, offset_y + pitchy);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 3, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 3);//横線
                                if (j <= 1 || j == 7 || j == 13) { gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy); }//縦線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * j + slide, offset_y + pitchy * 3);//縦線
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels3[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * 2, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 3);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { reac_f[i][0].Value.ToString("G"), reac_f[i][1].Value.ToString("F1"), reac_f[i][2].Value.ToString("F1"), reac_f[i][3].Value.ToString("F1"), reac_f[i][4].Value.ToString("F1"), reac_f[i][5].Value.ToString("F1"), reac_f[i][6].Value.ToString("F1") };
                        FxL += reac_f[i][1].Value; FyL += reac_f[i][2].Value; FzL += reac_f[i][3].Value;
                        MxL += reac_f[i][4].Value; MyL += reac_f[i][5].Value; MzL += reac_f[i][6].Value;
                        if (reac_f[0].Count >= 21)
                        {
                            values.Add(reac_f[i][7 + 1].Value.ToString("F1")); values.Add(reac_f[i][7 + 2].Value.ToString("F1")); values.Add(reac_f[i][7 + 3].Value.ToString("F1")); values.Add(reac_f[i][7 + 4].Value.ToString("F1")); values.Add(reac_f[i][7 + 5].Value.ToString("F1")); values.Add(reac_f[i][7 + 6].Value.ToString("F1"));
                            values.Add(reac_f[i][14 + 1].Value.ToString("F1")); values.Add(reac_f[i][14 + 2].Value.ToString("F1")); values.Add(reac_f[i][14 + 3].Value.ToString("F1")); values.Add(reac_f[i][14 + 4].Value.ToString("F1")); values.Add(reac_f[i][14 + 5].Value.ToString("F1")); values.Add(reac_f[i][14 + 6].Value.ToString("F1"));
                            FxX += reac_f[i][8].Value; FyX += reac_f[i][9].Value; FzX += reac_f[i][10].Value;
                            MxX += reac_f[i][11].Value; MyX += reac_f[i][12].Value; MzX += reac_f[i][13].Value;
                            FxY += reac_f[i][15].Value; FyY += reac_f[i][16].Value; FzY += reac_f[i][17].Value;
                            MxY += reac_f[i][18].Value; MyY += reac_f[i][19].Value; MzY += reac_f[i][20].Value;
                        }
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    //count += 1;
                    //var total_values = new List<string> { "sum", ((int)FxL).ToString("D"), ((int)FyL).ToString("D"), ((int)FzL).ToString("D"), ((int)MxL).ToString("D"), ((int)MyL).ToString("D"), ((int)MzL).ToString("D") };
                    //if (reac_f[0].Count >= 21) { total_values.Add(((int)FxX).ToString("D")); total_values.Add(((int)FyX).ToString("D")); total_values.Add(((int)FzX).ToString("D")); total_values.Add(((int)MxX).ToString("D")); total_values.Add(((int)MyX).ToString("D")); total_values.Add(((int)MzX).ToString("D")); total_values.Add(((int)FxY).ToString("D")); total_values.Add(((int)FyY).ToString("D")); total_values.Add(((int)FzY).ToString("D")); total_values.Add(((int)MxY).ToString("D")); total_values.Add(((int)MyY).ToString("D")); total_values.Add(((int)MzY).ToString("D")); }
                    //for (int j = 0; j < total_values.Count; j++)
                    //{
                    //    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                    //    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                    //    gfx.DrawString(total_values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                    //    if (j == labels.Count - 1)
                    //    {
                    //        j += 1;
                    //        gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                    //    }
                    //}
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_reac_f.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (sec_f1[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 32; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 8 + 5;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 0;
                    for (int i = 0; i < sec_f1.Count; i++)
                    {
                        var labels = new List<string> { "No.", "Ni", "Qyi", "Qzi", "Myi", "Mzi", "Nj", "Qyj", "Qzj", "Myj", "Mzj", "Nc", "Qyc", "Qzc", "Myc", "Mzc" };
                        var labels2 = new List<string> { "", "kN", "kN", "kN", "kNm", "kNm", "kN", "kN", "kN", "kNm", "kNm", "kN", "kN", "kN", "kNm", "kNm" };
                        int ni = (int)ij[i][0].Value; int nj = (int)ij[i][1].Value;
                        var l = Math.Sqrt(Math.Pow(r[ni][0].Value - r[nj][0].Value, 2) + Math.Pow(r[ni][1].Value - r[nj][1].Value, 2) + Math.Pow(r[ni][2].Value - r[nj][2].Value, 2));
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 1) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { i.ToString(), sec_f1[i][0].Value.ToString("F1"), sec_f1[i][1].Value.ToString("F1"), sec_f1[i][2].Value.ToString("F1"), sec_f1[i][4].Value.ToString("F1"), sec_f1[i][5].Value.ToString("F1"), sec_f1[i][6].Value.ToString("F1"), sec_f1[i][7].Value.ToString("F1"), sec_f1[i][8].Value.ToString("F1"), sec_f1[i][10].Value.ToString("F1"), sec_f1[i][11].Value.ToString("F1"), sec_f1[i][12].Value.ToString("F1"), sec_f1[i][13].Value.ToString("F1"), sec_f1[i][14].Value.ToString("F1"), sec_f1[i][16].Value.ToString("F1"), sec_f1[i][17].Value.ToString("F1") };
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_sec_f1.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (sec_f2[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 32; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 8 + 5;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 0;
                    for (int i = 0; i < sec_f2.Count; i++)
                    {
                        var labels = new List<string> { "No.", "Ni", "Qyi", "Qzi", "Myi", "Mzi", "Nj", "Qyj", "Qzj", "Myj", "Mzj", "Nc", "Qyc", "Qzc", "Myc", "Mzc" };
                        var labels2 = new List<string> { "", "kN", "kN", "kN", "kNm", "kNm", "kN", "kN", "kN", "kNm", "kNm", "kN", "kN", "kN", "kNm", "kNm" };
                        int ni = (int)ij[i][0].Value; int nj = (int)ij[i][1].Value;
                        var l = Math.Sqrt(Math.Pow(r[ni][0].Value - r[nj][0].Value, 2) + Math.Pow(r[ni][1].Value - r[nj][1].Value, 2) + Math.Pow(r[ni][2].Value - r[nj][2].Value, 2));
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 1) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { i.ToString(), sec_f2[i][0].Value.ToString("F1"), sec_f2[i][1].Value.ToString("F1"), sec_f2[i][2].Value.ToString("F1"), sec_f2[i][4].Value.ToString("F1"), sec_f2[i][5].Value.ToString("F1"), sec_f2[i][6].Value.ToString("F1"), sec_f2[i][7].Value.ToString("F1"), sec_f2[i][8].Value.ToString("F1"), sec_f2[i][10].Value.ToString("F1"), sec_f2[i][11].Value.ToString("F1"), sec_f2[i][12].Value.ToString("F1"), sec_f2[i][13].Value.ToString("F1"), sec_f2[i][14].Value.ToString("F1"), sec_f2[i][16].Value.ToString("F1"), sec_f2[i][17].Value.ToString("F1") };
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_sec_f2.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (sec_f3[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 32; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 8 + 5;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 0;
                    for (int i = 0; i < sec_f3.Count; i++)
                    {
                        var labels = new List<string> { "No.", "Ni", "Qyi", "Qzi", "Myi", "Mzi", "Nj", "Qyj", "Qzj", "Myj", "Mzj", "Nc", "Qyc", "Qzc", "Myc", "Mzc" };
                        var labels2 = new List<string> { "", "kN", "kN", "kN", "kNm", "kNm", "kN", "kN", "kN", "kNm", "kNm", "kN", "kN", "kN", "kNm", "kNm" };
                        int ni = (int)ij[i][0].Value; int nj = (int)ij[i][1].Value;
                        var l = Math.Sqrt(Math.Pow(r[ni][0].Value - r[nj][0].Value, 2) + Math.Pow(r[ni][1].Value - r[nj][1].Value, 2) + Math.Pow(r[ni][2].Value - r[nj][2].Value, 2));
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 1) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { i.ToString(), sec_f3[i][0].Value.ToString("F1"), sec_f3[i][1].Value.ToString("F1"), sec_f3[i][2].Value.ToString("F1"), sec_f3[i][4].Value.ToString("F1"), sec_f3[i][5].Value.ToString("F1"), sec_f3[i][6].Value.ToString("F1"), sec_f3[i][7].Value.ToString("F1"), sec_f3[i][8].Value.ToString("F1"), sec_f3[i][10].Value.ToString("F1"), sec_f3[i][11].Value.ToString("F1"), sec_f3[i][12].Value.ToString("F1"), sec_f3[i][13].Value.ToString("F1"), sec_f3[i][14].Value.ToString("F1"), sec_f3[i][16].Value.ToString("F1"), sec_f3[i][17].Value.ToString("F1") };
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_sec_f3.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (sec_f4[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 32; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 8 + 5;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 0;
                    for (int i = 0; i < sec_f4.Count; i++)
                    {
                        var labels = new List<string> { "No.", "Ni", "Qyi", "Qzi", "Myi", "Mzi", "Nj", "Qyj", "Qzj", "Myj", "Mzj", "Nc", "Qyc", "Qzc", "Myc", "Mzc" };
                        var labels2 = new List<string> { "", "kN", "kN", "kN", "kNm", "kNm", "kN", "kN", "kN", "kNm", "kNm", "kN", "kN", "kN", "kNm", "kNm" };
                        int ni = (int)ij[i][0].Value; int nj = (int)ij[i][1].Value;
                        var l = Math.Sqrt(Math.Pow(r[ni][0].Value - r[nj][0].Value, 2) + Math.Pow(r[ni][1].Value - r[nj][1].Value, 2) + Math.Pow(r[ni][2].Value - r[nj][2].Value, 2));
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 1) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { i.ToString(), sec_f4[i][0].Value.ToString("F1"), sec_f4[i][1].Value.ToString("F1"), sec_f4[i][2].Value.ToString("F1"), sec_f4[i][4].Value.ToString("F1"), sec_f4[i][5].Value.ToString("F1"), sec_f4[i][6].Value.ToString("F1"), sec_f4[i][7].Value.ToString("F1"), sec_f4[i][8].Value.ToString("F1"), sec_f4[i][10].Value.ToString("F1"), sec_f4[i][11].Value.ToString("F1"), sec_f4[i][12].Value.ToString("F1"), sec_f4[i][13].Value.ToString("F1"), sec_f4[i][14].Value.ToString("F1"), sec_f4[i][16].Value.ToString("F1"), sec_f4[i][17].Value.ToString("F1") };
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_sec_f4.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (spring_f1[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 29; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 6;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 0;
                    var labels = new List<string> { "No.", "N", "Qy", "Qz", "My", "Mz" };
                    var labels2 = new List<string> { "", "[kN]", "[kN]", "[kN]", "[kNm]", "[kNm]" };
                    for (int i = 0; i < spring_f1.Count; i++)
                    {
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 3) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy * 2), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { i.ToString(), spring_f1[i][0].Value.ToString("F1"), spring_f1[i][1].Value.ToString("F1"), spring_f1[i][2].Value.ToString("F1"), spring_f1[i][4].Value.ToString("F1"), spring_f1[i][5].Value.ToString("F1")};
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_spring_f1.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (spring_f2[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 29; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 6;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 0;
                    var labels = new List<string> { "No.", "N", "Qy", "Qz", "My", "Mz" };
                    var labels2 = new List<string> { "", "[kN]", "[kN]", "[kN]", "[kNm]", "[kNm]" };
                    for (int i = 0; i < spring_f2.Count; i++)
                    {
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 3) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy * 2), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { i.ToString(), spring_f2[i][0].Value.ToString("F1"), spring_f2[i][1].Value.ToString("F1"), spring_f2[i][2].Value.ToString("F1"), spring_f2[i][4].Value.ToString("F1"), spring_f2[i][5].Value.ToString("F1") };
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_spring_f2.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (spring_f3[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 29; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 6;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 0;
                    var labels = new List<string> { "No.", "N", "Qy", "Qz", "My", "Mz" };
                    var labels2 = new List<string> { "", "[kN]", "[kN]", "[kN]", "[kNm]", "[kNm]" };
                    for (int i = 0; i < spring_f3.Count; i++)
                    {
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 3) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy * 2), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { i.ToString(), spring_f3[i][0].Value.ToString("F1"), spring_f3[i][1].Value.ToString("F1"), spring_f3[i][2].Value.ToString("F1"), spring_f3[i][4].Value.ToString("F1"), spring_f3[i][5].Value.ToString("F1") };
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_spring_f3.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (spring_f4[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 29; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 6;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 0;
                    var labels = new List<string> { "No.", "N", "Qy", "Qz", "My", "Mz" };
                    var labels2 = new List<string> { "", "[kN]", "[kN]", "[kN]", "[kNm]", "[kNm]" };
                    for (int i = 0; i < spring_f4.Count; i++)
                    {
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 3) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy * 2), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { i.ToString(), spring_f4[i][0].Value.ToString("F1"), spring_f4[i][1].Value.ToString("F1"), spring_f4[i][2].Value.ToString("F1"), spring_f4[i][4].Value.ToString("F1"), spring_f4[i][5].Value.ToString("F1") };
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_spring_f4.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
                if (spring_f5[0][0].Value != -9999)
                {
                    // PDFドキュメントを作成。
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = pdfname;
                    document.Info.Author = "Shinnosuke Fujita, Assoc. Prof., The Univ. of Kitakyushu shinnosuke@dn-archi.com";
                    var label_width = 29; var offset_x = 25; var offset_y = 25; var pitchy = 13; var text_width = 20; var lines = 50; var clear = label_width * 6;
                    PdfPage page = new PdfPage(); page.Size = PageSize.A4; var pen = XPens.Black;
                    var slide = -clear; var count = 0;
                    var labels = new List<string> { "No.", "N", "Qy", "Qz", "My", "Mz" };
                    var labels2 = new List<string> { "", "[kN]", "[kN]", "[kN]", "[kNm]", "[kNm]" };
                    for (int i = 0; i < spring_f5.Count; i++)
                    {
                        if (i % lines == 0)
                        {
                            count = 1;
                            if (i % (lines * 3) == 0)
                            {
                                page = document.AddPage(); gfx = XGraphics.FromPdfPage(page); slide = -clear;
                            }
                            slide += clear;
                            for (int j = 0; j < labels.Count; j++)//ラベル列**************************************************************************
                            {
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * (j + 1) + slide, offset_y);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * 2, offset_x + label_width * (j + 1) + slide, offset_y + pitchy * 2);//横線
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                gfx.DrawString(labels[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y, label_width, offset_y + pitchy), XStringFormats.TopCenter);
                                gfx.DrawString(labels2[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy, label_width, offset_y + pitchy * 2), XStringFormats.TopCenter);
                                if (j == labels.Count - 1)
                                {
                                    j += 1;
                                    gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y, offset_x + label_width * j + slide, offset_y + pitchy * 2);//縦線
                                }
                            }
                        }
                        count += 1;
                        var values = new List<string> { i.ToString(), spring_f5[i][0].Value.ToString("F1"), spring_f5[i][1].Value.ToString("F1"), spring_f5[i][2].Value.ToString("F1"), spring_f5[i][4].Value.ToString("F1"), spring_f5[i][5].Value.ToString("F1") };
                        for (int j = 0; j < values.Count; j++)
                        {
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1), offset_x + label_width * (j + 1) + slide, offset_y + pitchy * (count + 1));//横線
                            gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            gfx.DrawString(values[j], font, XBrushes.Black, new XRect(offset_x + label_width * j + slide, offset_y + pitchy * count, label_width, offset_y + pitchy * (count + 1)), XStringFormats.TopCenter);
                            if (j == labels.Count - 1)
                            {
                                j += 1;
                                gfx.DrawLine(pen, offset_x + label_width * j + slide, offset_y + pitchy * count, offset_x + label_width * j + slide, offset_y + pitchy * (count + 1));//縦線
                            }
                        }
                    }
                    var dir = Path.GetDirectoryName(Rhino.RhinoDoc.ActiveDoc.Path);
                    // ドキュメントを保存。
                    var filename = dir + "/" + pdfname + "_spring_f5.pdf";
                    document.Save(filename);
                    // ビューアを起動。
                    Process.Start(new ProcessStartInfo(@filename) { UseShellExecute = true });
                }
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
                return OpenSeesUtility.Properties.Resources.dataoutput;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7d21e116-63a1-4ef6-8d53-70d53f0f4c4b"); }
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
                int height = 17; int radi1 = 7; int radi2 = 4;
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