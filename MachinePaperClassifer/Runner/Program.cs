﻿// See https://aka.ms/new-console-template for more information

using System.Data;
using System.Reflection;
using IronXL;
using Runner.lib;




List<TargetModel> targetModels = new List<TargetModel>();
targetModels.Add(TargetModel.A);
targetModels.Add(TargetModel.B);

foreach (var targetModel in targetModels)
{
    var filesToBeLoaded = DataSetOperation.readDirectoryInfo(targetModel);

    var dataSets = new List<DataSet>();
    foreach (var files in filesToBeLoaded)
    {
        var dataSet = FileOperation.readExcel(files);
        var sheet = dataSet.Tables[0];
        var sourcePdfFileList = DataSetOperation.generateSourcePDFInfo(sheet);
        var targetPdfFilePath = DataSetOperation.generatePDFDesinationPath(targetModel);
        foreach (var pdfFileName in sourcePdfFileList)
        {
            Directory.CreateDirectory(targetPdfFilePath);
            string sourcePDFPath = DataSetOperation.path+"/二维PDF/";
            try
            {
                File.Copy(sourcePDFPath + "/" + pdfFileName, targetPdfFilePath + "/" + pdfFileName,true);
            }
            catch (FileNotFoundException exception)
            {
                Console.WriteLine("在信息文档"+files.Replace(DataSetOperation.path,"")+"找不到文件："+pdfFileName);
            }
        }

    }
}





//Console.WriteLine(distinctCategories);