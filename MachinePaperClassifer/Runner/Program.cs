// See https://aka.ms/new-console-template for more information

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

    var dataSets = new List<List<DataSetOperation.DrawingInfo>>();
    foreach (var files in filesToBeLoaded)
    {
        var dataSet = FileOperation.readExcel(files);
        var sheet = dataSet.Tables[0];
        
        var pdfOverallSrcInfo = DataSetOperation.generateSourceInfo(sheet,"Column5 not like '%外购%' and Column1 is not null and Column1 not like '%零件号%'");
        var overallSrcInfo = DataSetOperation.generateSourceInfo(sheet,"Column1 is not null and Column1 not like '%零件号%'");
        dataSets.Add(overallSrcInfo);
        var pdfSrcInfo = pdfOverallSrcInfo.DistinctBy(value => value.filename).ToList();
        
        
        var targetPdfFilePath = DataSetOperation.generatePDFDesinationPath(targetModel,"PDF图纸分类");
       
        foreach (var pdfInfo in pdfSrcInfo)
        {  // Console.WriteLine(pdfInfo.filename+","+pdfInfo.category+","+pdfInfo.count()+","+pdfInfo.countUnit()+"，"+pdfInfo.level.Length+"--"+pdfInfo.className);
            Directory.CreateDirectory(targetPdfFilePath +"/"+pdfInfo.className+  "/"+ pdfInfo.category);
            string sourcePDFPath = DataSetOperation.path+"/二维PDF/";
            try
            {
                File.Copy(sourcePDFPath + "/" + pdfInfo.filename+".pdf", targetPdfFilePath +"/"+ pdfInfo.className + "/"+ pdfInfo.category+ "/" + pdfInfo.filename+".pdf",true);
            }
            catch (FileNotFoundException exception)
            {
                Console.WriteLine("在信息文档"+files.Replace(DataSetOperation.path,"")+"找不到文件："+pdfInfo.filename);
            }
        }
        
        //foreach(var )
        
    }
    DataSetOperation.generateDetailByClass(targetModel, dataSets);

}





//Console.WriteLine(distinctCategories);