using UnityEngine;
using MiniExcelLibs;
using System.IO;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Game.Runtime
{
    public class DemoRuntime : MonoBehaviour
    {
        private async UniTask Start()
        {
            var path = Path.Combine(Application.dataPath, "..", "Resources/MasterData.xlsx");
            using var stream = File.OpenRead(path);
            var sheetNames = new List<string>();
            MiniExcel.GetSheetNames(stream, sheetNames, x => x.StartsWith("<WIP>") == false);
            
            foreach (var sheetName in sheetNames)
            {
                Debug.Log(sheetName);

                var csvPath = $"{Application.dataPath}/CSV/{sheetName}";

                if (Directory.Exists(csvPath) == false)
                    Directory.CreateDirectory(csvPath);

                using (var csvStream = new FileStream(csvPath, FileMode.CreateNew))
                {
                    var rows = await MiniExcel.QueryAsync(path, false, sheetName, ExcelType.XLSX);
                    await MiniExcel.SaveAsAsync(csvStream, rows, printHeader: false, excelType: ExcelType.CSV);
                }
            }
        }
    }
}
