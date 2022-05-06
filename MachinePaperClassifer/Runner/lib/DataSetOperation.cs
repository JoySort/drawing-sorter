using System.Data;
using System.Reflection;
using System.Text;

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

    public static List<DrawingInfo> generateSourceInfo(DataTable table,string filter)
    {

        var cname = table.Rows[0].ItemArray[0];
        var classCount = table.Rows[0].ItemArray[4];
        if(DBNull.Value==cname)cname = table.Rows[1].ItemArray[0];
        if(DBNull.Value==classCount)classCount=table.Rows[1].ItemArray[4];
        
        var rows = table
            .Select(filter);
            
        var currentTasks = (from DataRow dr in rows
                select new DrawingInfo
                {
                    filename = dr["Column1"].ToString(),
                    category = dr["Column5"].ToString().Replace(" ","").Replace("/","-"),
                    _countString = dr["Column4"].ToString(),
                    _level = dr["Column0"].ToString(),
                    _classCount=(string)classCount,
                    className = cname==DBNull.Value?"":(string)cname
                }
                );
        return currentTasks.ToList(); //.DistinctBy(value=>value.filename).ToList();
    }
    

    public class DrawingInfo
    {
        public string _classCount;

        public int classCount()
        {
            var internalString= new string(_classCount.Where(value=>Char.IsDigit(value)).ToArray());
            return  int.Parse(internalString);
        }

        public string _level;
        public int[] level
        {
            get => _level != null ?_level.Split(".").Select(value=>int.Parse(value)).ToArray() : new int[0];
        }
        public string filename;
        public string category;
        public string className;
        public string _countString;

        public string countUnit()
        {
           var temp= new string(_countString.Where(value=>!Char.IsDigit(value)).ToArray());
           return temp.Length == 0 ? "件" : temp;
        }

        public  int  count()
        {
          var internalString= new string(_countString.Where(value=>Char.IsDigit(value)).ToArray());
          // if(int.Parse(internalString)>1)
          //    Console.WriteLine($"{filename},{internalString}");
           return  int.Parse(internalString);
        }
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

    public static string generatePDFDesinationPath(TargetModel tmodel,string subModule)
    {
        string destinationPDFPath = path + "/生产应用/";
        string result = null;
        switch (tmodel){
        case TargetModel.A:
            result = destinationPDFPath + "A款/"+subModule+"/";
         break;
        case TargetModel.B:
            result = destinationPDFPath + "B款/"+subModule+"/";
         break;
         default:
         break;
        }

        return result;
    }


    public static void generateDetailByClass(TargetModel targetModel, List<List<DataSetOperation.DrawingInfo>> dataSets)
    {
        var targetDetailFilePath = DataSetOperation.generatePDFDesinationPath(targetModel,"明细文件");


        var overallCsv = new List<CSVHolder>();
        foreach (var dataset in dataSets)
        {
            var groupByDrawingNo = dataset
                .GroupBy(value => value.filename).Select(grp => grp.ToList());
            var className = dataset.First().className;
            Console.WriteLine($"处理模块{className}");
            var levelMap = dataset.ToDictionary(value => string.Join(",", value.level.Select(x => x.ToString()).ToArray()), value => value);
            
            var countDictionaryByDrawingNo = new Dictionary<string, Dictionary<string, DetailDrawingStats>>();

            
            var csvContent = new Dictionary<string,List<CSVHolder>>();
            foreach (var drawingInfoWithSameDrawingNo in groupByDrawingNo)
            {
                //var drawingInfoWithSameDrawingNo.OrderBy(value=>value.level[0]).ThenBy(value=>value.level[1]).ThenBy(value=>value.level[2])
                var drawingNoCount = 0;
                var drawingNoPath = new List<string>();
                
                if ( drawingInfoWithSameDrawingNo.First().filename=="GB／T70．1-2000内六角圆柱头螺钉M8×55")
                {
                   // Console.WriteLine("");
                }
                
                    foreach (var item in drawingInfoWithSameDrawingNo)
                    {
                        var countResult = item.classCount();
                       // if(className.Contains("CXZJV12-12-00-吹气组件")|| className.Contains("选枣通道"))countResult = 7;
                        for (var i = item.level.Length; i > 0; i--)
                        {
                           
                            var key =  string.Join(",", item.level[0..i].Select(x => x.ToString()).ToArray());
                            countResult = levelMap.ContainsKey(key)? levelMap[key].count()  * countResult:countResult;
                        }

                        drawingNoCount += countResult;
                        drawingNoPath.Add(item.filename);
                    }

                    

                    var result = new CSVHolder();
                    result.category = drawingInfoWithSameDrawingNo.First().category;
                    result.className = className;
                    result.drawingNo = drawingInfoWithSameDrawingNo.First().filename;
                    result.count = drawingNoCount;
                    drawingNoPath.Reverse();
                    result.drawingNoPath = drawingNoPath.Aggregate((i, j) => i + "=>" + j);
                    
                  
                    
                    if(!csvContent.ContainsKey(result.category))csvContent.Add(result.category,new List<CSVHolder>());
                    csvContent[result.category].Add(result);
                    overallCsv.Add(result);
                    
            }

            foreach ((var key, var csvHolders) in csvContent)
            {
                var lines = csvHolders.Select(value=>value.content).ToList();
                var targetPath = targetDetailFilePath + "/" + className+"/";
                Directory.CreateDirectory(targetPath);
                File.WriteAllLines(targetPath+key+".csv", lines,Encoding.UTF8);
            }

           
            

        }
        
        var purchaseList = overallCsv.Where(x => x.category.Contains("购"));
        var distinctDrawingNoList = purchaseList.GroupBy(value => value.drawingNo);
        var aggregatedList = new List<CSVHolder>();
        foreach (var csvHolderList in distinctDrawingNoList)
        {
            var csvHolderTmp = new CSVHolder();
            csvHolderTmp.drawingNo = csvHolderList.Key;
            csvHolderTmp.category = "";
            csvHolderTmp.className = "";
            csvHolderTmp.drawingNoPath = "";
            csvHolderTmp.count = 0;
            foreach (var csvHolder in csvHolderList)
            {
                csvHolderTmp.category =  csvHolder.category;
                csvHolderTmp.className = csvHolderTmp.className +" | "+ csvHolder.className +"*"+ csvHolder.count;
                csvHolderTmp.drawingNoPath = csvHolderTmp.drawingNoPath  +" | "+  csvHolder.drawingNoPath;
                csvHolderTmp.count = csvHolderTmp.count + csvHolder.count;
                
                
            }
            aggregatedList.Add(csvHolderTmp);

        }
        var ls =  aggregatedList.Select(value => value.content);
        //
        var tp = targetDetailFilePath + "/" ;
        Directory.CreateDirectory(tp);
        File.WriteAllLines(tp+"外购总表"+".csv", ls,Encoding.UTF8);


    }

    // public static void generateDetail(TargetModel targetModel,List<List<DataSetOperation.DrawingInfo>> dataSets)
    // {
    //     var targetDetailFilePath = DataSetOperation.generatePDFDesinationPath(targetModel,"明细文件");
    //
    //     var finalList = new List<DataSetOperation.DrawingInfo>();
    //     foreach (var drawingList in dataSets)
    //     {
    //         finalList.AddRange(drawingList);
    //     }
    //
    //     var groupByDrawingNo = finalList
    //         .GroupBy(value => value.filename).Select(grp => grp.ToList());
    //         
    //     
    //   
    //     var countDictionaryByDrawingNo = new Dictionary<string, Dictionary<string, DetailDrawingStats>>();
    //     foreach (var drawingInfos in groupByDrawingNo)
    //     {
    //
    //         var count = 0;
    //
    //         var groupedDictionary = new Dictionary<string, List<DataSetOperation.DrawingInfo>>();
    //         foreach (var drawInfo in drawingInfos)
    //         {
    //             if(!(groupedDictionary.ContainsKey(drawInfo.className)))
    //                 groupedDictionary.Add(drawInfo.className, new List<DataSetOperation.DrawingInfo>());
    //             groupedDictionary[drawInfo.className].Add(drawInfo);
    //         }
    //
    //         var countResultDictionaryByClass = new Dictionary<string, DetailDrawingStats>();
    //         
    //         foreach ((var className,var drawItems )in groupedDictionary)
    //         {
    //             var detailDrawingStats = new DetailDrawingStats();
    //             
    //             detailDrawingStats.className = className;
    //             detailDrawingStats._drawingInfos = drawItems
    //                 .OrderBy(value => value.level[0])
    //                 .ThenBy(value => value.level.Length>=2?value.level[1]:-1)
    //                 .ThenBy(value => value.level.Length>=3?value.level[2]:-1)
    //                 .ToList();
    //             detailDrawingStats.drawingNo = drawItems.First().filename;
    //             if(drawingInfos.First().filename!=detailDrawingStats.drawingNo) throw new Exception("Incorrect grouping by drawing NO");
    //             var drawingByClassLevelDictionary = drawItems.ToDictionary(value => value.level, value => value);
    //             var countResult = 1;
    //             foreach (var item in drawItems)
    //             {
    //                 
    //                 for (var i = item.level.Length-1; i >= 0; i--)
    //                 {
    //                     var key = item.level[0..i];
    //                     countResult = drawingByClassLevelDictionary.ContainsKey(key)? drawingByClassLevelDictionary[key].count() * countResult:countResult;
    //                 }
    //
    //                 if (detailDrawingStats.drawingNo != item.filename)
    //                     throw new Exception("Incorrect grouping by drawing NO");
    //                 
    //                 
    //             }
    //
    //             detailDrawingStats.count = countResult;
    //             countResultDictionaryByClass.Add(className,detailDrawingStats);
    //         }
    //         
    //         countDictionaryByDrawingNo.Add(drawingInfos.First().filename,countResultDictionaryByClass);
    //     }
    //
    //     var drawingNoClassMapping = finalList.DistinctBy(value=>value.filename).ToDictionary(value => value.filename, value => value.category);
    //     var wrinteFileContent = new List<CSVHolder>();
    //     
    //     foreach ((var drawingNo, var drawingDetailDic) in countDictionaryByDrawingNo)
    //     {
    //         var count = 0;
    //         var drawingPath = "";
    //         foreach((var className,var drawingDetail) in drawingDetailDic)
    //         {
    //             count += drawingDetail.count;
    //             drawingPath += "[["+drawingDetail.drawingPath()+"]]";
    //         }
    //
    //
    //         var line = $"{Enum.GetName(targetModel)},{drawingNoClassMapping[drawingNo]},{count},{drawingNo},{drawingPath}";
    //         
    //         
    //         wrinteFileContent.Add(line);
    //         if(!wrinteFileContent.ContainsKey(drawingNoClassMapping[drawingNo]))wrinteFileContent.Add(drawingNoClassMapping[drawingNo],new List<string>());
    //         wrinteFileContent[drawingNoClassMapping[drawingNo]].Add(line);
    //         Console.WriteLine(line);
    //         
    //     }
    //
    //     foreach ((var key, var value) in wrinteFileContent)
    //     {
    //         Directory.CreateDirectory(targetDetailFilePath);
    //         File.WriteAllLines(targetDetailFilePath+key+".csv", value,Encoding.UTF8);
    //     }
    //     //
    //     
    // }

}

public class CSVHolder
{
    public string className;
    public string drawingNo;
    public string category;
    public int count;
    public string content => $"{drawingNo},{count},{category},{className},{drawingNoPath}";
    public string drawingNoPath;
}

public enum TargetModel
{
    A,
    B
}

public class DetailDrawingStats
{
    public List<DataSetOperation.DrawingInfo> _drawingInfos = new List<DataSetOperation.DrawingInfo>();
    public string className;
    public string drawingNo;
    public int count;

    public string drawingPath()
    {
        var result = "[";
        foreach (var item in _drawingInfos)
        {
            result += item.className+"==>"+ item.filename + "      ";
        }

        result += "]";
        return result;
    }

}