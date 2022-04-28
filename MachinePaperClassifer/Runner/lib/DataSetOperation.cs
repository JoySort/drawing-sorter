using System.Data;
using System.Reflection;

namespace Runner.lib;

public class DataSetOperation
{
    public static List<string> DistinctCategories(DataTable table)
    {
        var categoryExclude = new List<string>();
        categoryExclude.Add(null);
        categoryExclude.Add("");
        categoryExclude.Add("/备注");
        categoryExclude.Add("备注");
    
        var categories = table.AsEnumerable().Select(dataRow => dataRow.Field<string>("Column5")).ToList().Distinct().ToList().Except(categoryExclude).ToList();

        return categories;

    }

    public static List<string> generateSourcePDFInfo(DataTable table)
    {
        
        
        var excludeValue = new List<string>();
        excludeValue.Add(null);
        excludeValue.Add("");
        excludeValue.Add("零件号");
        var row = table
            .Select("Column5 not like '%外购%' ");
            
        var fileList = row.Select(dataRow => dataRow.Field<string>("Column1")).ToList()
            .Except(excludeValue).ToList().Select(value=>value+".pdf").ToList();
        return fileList;
    }

    
    private static string originPath = "../";
    public static string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,originPath);
    
    private static string[] filePaths = Directory.GetFiles(path + "/二维PDF");

    
    
    
   
    
    
    
    public static string[] readDirectoryInfo(TargetModel targetModel)
    {
        
         string srcModelAPath = path+"/明细/A款明细/";
         string srcModelBPath = path+"/明细/B款明细/";
         string modelSharePath = path+ "/明细/A款明细/共用明细/";
        
        string[] result = null;
        switch (targetModel){
            case TargetModel.A:
                result = Directory.EnumerateFiles(modelSharePath,"*.*",SearchOption.AllDirectories).Concat(Directory.EnumerateFiles(srcModelAPath,"*.*"))
                    .Where(s=>!s.Contains("~$") && (s.ToLower().EndsWith(".xlsx") || s.ToLower().EndsWith(".xls"))).ToArray();
                break;
            case TargetModel.B:
                result = Directory.EnumerateFiles(modelSharePath,"*.*",SearchOption.AllDirectories).Concat(Directory.EnumerateFiles(srcModelBPath,"*.*",SearchOption.AllDirectories))
                    .Where(s=>!s.Contains("~$") && (s.ToLower().EndsWith(".xlsx") || s.ToLower().EndsWith(".xls"))).ToArray();
                break;
            default:
                break;
        }

        return result;

    }

    public static string generatePDFDesinationPath(TargetModel tmodel)
    {
        string destinationPDFPath = path + "/PDF图纸分类";
        string result = null;
        switch (tmodel){
        case TargetModel.A:
            result = destinationPDFPath + "A款/";
         break;
        case TargetModel.B:
            result = destinationPDFPath + "B款/";
         break;
         default:
         break;
        }

        return result;
    }
}

public enum TargetModel
{
    A,
    B
}