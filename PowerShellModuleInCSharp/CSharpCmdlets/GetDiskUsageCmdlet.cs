﻿﻿using PowerShellModuleInCSharp.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.UI;

namespace PowerShellModuleInCSharp.CSharpCmdlets
{
    [Cmdlet(VerbsCommon.Get, nameof(DiskUsage))]
    [OutputType(typeof(DiskUsage))]
    public class GetDiskUsageCmdlet : Cmdlet
    {
        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        [Alias("disk")]
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, ValueFromPipeline = true)]
        public string Drive { get; set; }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
        public string Export { get; set; }

        private DateTime Date { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();



            DriveInfo drive = new DriveInfo(Drive);
            Console.WriteLine("Total Free Space: " + drive.TotalFreeSpace/(1024*1024*1024) + "GB");
            Console.WriteLine("Volume Label: "+drive.VolumeLabel);

            File.WriteAllText(Export,TraverseTree(Drive));
            



        }

        public static string TraverseTree(string root)
        {
            Stack<string> dirs = new Stack<string>(20);

            StringWriter stringWriter = new StringWriter();
            HtmlTextWriter writer = new HtmlTextWriter(stringWriter);

            
            if (!Directory.Exists(root))
            {
                throw new ArgumentException();

            }
            dirs.Push(root);
            DirectoryInfo tempDirectory = new DirectoryInfo(root);
            int dirDiff = 0;

            
            
            writer.RenderBeginTag(HtmlTextWriterTag.H1);
            writer.Write(new DirectoryInfo(root).Name);


            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs;
                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                foreach (string dir in subDirs)
                    dirs.Push(dir);
                

                dirDiff = tempDirectory.FullName.Split('\\').Length - 
                    new DirectoryInfo(currentDir).FullName.Split('\\').Length;

                if (dirDiff >= 0)
                {
                    for(int i = 0; i <= dirDiff; i++)
                    {
                        writer.RenderEndTag();
                        
                    }
                }

           
                tempDirectory = new DirectoryInfo(currentDir);

                string display = tempDirectory.FullName.Split('\\').Length -
                    new DirectoryInfo(root).FullName.Split('\\').Length > 3 ? "none" : "inline-block"; //Show directory depth.


                long folderSize = (long)GetFolderSize(new DirectoryInfo(currentDir));
                
                writer.AddAttribute(HtmlTextWriterAttribute.Id,
                    Directory.GetParent(currentDir).Name + "_" + new DirectoryInfo(currentDir).Name);
                writer.AddAttribute(HtmlTextWriterAttribute.Style, 
                    "border: 1px solid black; " +
                    "padding: 4px; " +
                    "float: left; " +
                    "text-align: left; " +
                    "font-size: 15px;" +
                    "overflow: hidden;" +
                    "width:" + folderSize/100 + "px;" +
                    "height:" + folderSize/100 + "px;" +
                    "display: " + display + ";");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(new DirectoryInfo(currentDir).Name);
                writer.Write(folderSize/1000 + "GB");



                string[] files = null;
                
                try
                {
                    
                    files = Directory.GetFiles(currentDir);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                foreach (string file in files)
                {
                    try
                    {
                        FileInfo fi = new FileInfo(file);
                        double fileSize = (fi.Length / 1024f) / 1024f;
                        
                        if (fileSize >= 1)
                        {
                            //fileSize = fileSize > 1000 ? 30 : 10;
                            writer.AddAttribute(HtmlTextWriterAttribute.Id, fi.Name);
                            writer.AddAttribute(HtmlTextWriterAttribute.Style,
                                "border: 1px solid black; " +
                                "padding: 4px; " +
                                "float: left; " +
                                "display: flex; " +
                                "text-align: left; " +
                                "font-size: 15px;" +
                                "width: " + (long)fileSize/100  + "px;" +
                                "height: " + (long)fileSize/100  + "px;" +
                                "background: lightblue;" +
                                "overflow: hidden;");
                            writer.RenderBeginTag(HtmlTextWriterTag.Div);
                            writer.Write(fi.Name);
                            writer.RenderEndTag();
                        }

                        //Console.WriteLine(fi.FullName);
                    }
                    catch(FileNotFoundException e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                }


            }

            dirDiff = tempDirectory.FullName.Split('\\').Length -
                    new DirectoryInfo(root).FullName.Split('\\').Length;

            for (int i = 0; i <= dirDiff; i++)
                writer.RenderEndTag();

            return stringWriter.ToString();


        }
        
        public static float GetFolderSize(DirectoryInfo path)
        {

            FileInfo[] files = null;
            DirectoryInfo[] subDirs = null;

            float folderSize = 0.0f;


            // process all the files directly under this folder
            try
            {
                files = path.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            if (files != null)
            {
                foreach (FileInfo file in files)
                {
                    float fileSize = (file.Length / 1024f) / 1024f;
                    folderSize += fileSize;
                }

            }

            try
            {
                subDirs = path.GetDirectories();
                foreach (DirectoryInfo subDir in subDirs)
                {
                    // Resursive call for each subdirectory.
                    folderSize += GetFolderSize(subDir);
                }

            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            return folderSize;

        }

       
    }
}
