#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using System.Reflection;
using System.Globalization;
using System.Resources;
#endregion

namespace FamilyCompactor
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        UIApplication uiApp;
        //UIDocument uiDoc;
        Application app;
        //Document doc;
#if REVIT2019
        string projectTemplateFileName = Path.Combine(Path.GetDirectoryName(Uri.UnescapeDataString((new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path))), "Clean2019.rte");
#elif REVIT2020
        string projectTemplateFileName = Path.Combine(Path.GetDirectoryName(Uri.UnescapeDataString((new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path))), "Clean2020.rte");
#elif REVIT2021
        string projectTemplateFileName = Path.Combine(Path.GetDirectoryName(Uri.UnescapeDataString((new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path))), "Clean2021.rte");
#elif REVIT2022
        string projectTemplateFileName = Path.Combine(Path.GetDirectoryName(Uri.UnescapeDataString((new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path))), "Clean2022.rte");
#elif REVIT2023
        string projectTemplateFileName = Path.Combine(Path.GetDirectoryName(Uri.UnescapeDataString((new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path))), "Clean2023.rte");
#elif REVIT2024
        string projectTemplateFileName = Path.Combine(Path.GetDirectoryName(Uri.UnescapeDataString((new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path))), "Clean2024.rte");
#elif REVIT2025
        string projectTemplateFileName = Path.Combine(Path.GetDirectoryName(Uri.UnescapeDataString((new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path))), "Clean2025.rte");
#endif

        ResourceManager rm = new ResourceManager($"{nameof(FamilyCompactor)}.Resources.strings", typeof(Command).Assembly);
        CultureInfo ci;
        string cultureName = "en";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiApp = commandData.Application;
            app = uiApp.Application;
            
            if (app.Language == LanguageType.English_USA ||
                app.Language == LanguageType.English_GB ||
                app.Language == LanguageType.Russian)
                cultureName = app.Language.ToString().Substring(0, 2);
            ci = CultureInfo.CreateSpecificCulture(cultureName);

            //uiDoc = uiApp.ActiveUIDocument;
            //doc = uiDoc.Document;

            //string test = Interaction.InputBox("Enter family paths (; - separator)");
            //string fileName = GetNextBackupFilePath(test);
            //TaskDialog.Show("1", fileName);

            CompactFamily();

            return Result.Succeeded;
        }

        public void CompactFamily()
        {
            try
            {
                FileOpenWindow fow = new FileOpenWindow($"Family compactor v{Assembly.GetExecutingAssembly().GetName().Version}", cultureName);
                if (fow.ShowDialog() == false) return;
                //string listOfFamilies = Microsoft.VisualBasic.Interaction.InputBox("Enter family paths (; - separator)");
                //if (listOfFamilies == "") return;
                //List<string> files = listOfFamilies.Split(';').ToList();
                List<string> files = fow.FilesList;
                string s1 = "";
                int i = 0;
                long profit = 0;
                List<string> openedDocumentsNames = new List<string>();
                foreach (Document d in app.Documents)
                    openedDocumentsNames.Add(d.Title);
                foreach (string s in files)
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(s);
                    if(!openedDocumentsNames.Contains(fileNameWithoutExtension))
                    {
                        if (CompactFamily(s, ref profit))
                        {
                            //s1 += "\"" + fileNameWithoutExtension + "\" compacted by " + profit + " bytes.\n";
                            //s1 += "\"" + fileNameWithoutExtension + "\" уменьшено на " + profit + " байт.\n";
                            s1 += String.Format(rm.GetString("FamilyCompactedBy", ci), fileNameWithoutExtension, profit) + '\n';
                            i++;
                        }
                        else
                            //s1 += "\"" + fileNameWithoutExtension + "\" could not be compacted.\n";
                            //s1 += "\"" + fileNameWithoutExtension + "\" не может быть уменьшено.\n";
                            s1 += String.Format(rm.GetString("FamilyCouldntBeCompacted", ci), fileNameWithoutExtension) + '\n';
                    }
                    else
                        //s1 += "\"" + fileNameWithoutExtension + "\" should be closed before compacting.\n";
                        //s1 += "\"" + fileNameWithoutExtension + "\" нужно закрыть перед обработкой.\n";
                        s1 += String.Format(rm.GetString("FamilyShoulBeClosed", ci), fileNameWithoutExtension) + '\n';
                };
                s1 += String.Format(rm.GetString("FilesCompacted", ci), i);
                TaskDialog.Show(nameof(FamilyCompactor), s1);
            }
            catch (Exception ex)
            {
                TaskDialog.Show(nameof(FamilyCompactor), ex.ToString());
            }
        }


        public bool CompactFamily(string familyPath, ref long profit)
        {
            //if (!doc.IsFamilyDocument) return;
            bool result = false;

            //string s = "";
            Document projectDoc = null;
            Document familyDocToSaveAs = null;
            Transaction t = null;
            string tempFamilyPath1 = Path.Combine(Path.GetDirectoryName(familyPath), Path.GetFileNameWithoutExtension(familyPath) + "_saveas_" + Guid.NewGuid().ToString() + Path.GetExtension(familyPath));
            string tempFamilyPath2 = Path.Combine(Path.GetDirectoryName(familyPath), Path.GetFileNameWithoutExtension(familyPath) + "_fromdoc_" + Guid.NewGuid().ToString() + Path.GetExtension(familyPath));

            try
            {
                // Пробуем уменьшить размер, сохранив семейство под другим именем
                if (!File.Exists(familyPath)) return false;
                familyDocToSaveAs = app.OpenDocumentFile(familyPath);
                if (familyDocToSaveAs == null) throw new Exception(rm.GetString("ErrorOpeningFamily", ci));
                familyDocToSaveAs.SaveAs(tempFamilyPath1, new SaveAsOptions() { Compact = true });
                familyDocToSaveAs.Close(false);
                if (!File.Exists(tempFamilyPath1)) throw new Exception(rm.GetString("ErrorSavingFamily", ci));


                File.Copy(familyPath, tempFamilyPath2);
                if (!File.Exists(tempFamilyPath2)) return false;
                //string txtPath = Path.ChangeExtension(familyPath, "txt"); // Путь к файлу каталога типов
                //string tempTxtPath = string.Empty; // Путь к временной резервной копии файла каталога типов

                projectDoc = app.NewProjectDocument(projectTemplateFileName);
                if (projectDoc == null) throw new Exception(rm.GetString("ErrorСreatingProjectDocument", ci));

                // Если файл каталога типов существует, временно переименовать его, чтоб они не грузились в проект
                //				int i = 0;
                //				if (File.Exists(txtPath))
                //					tempTxtPath = txtPath + "_temp.txt";
                //				while (File.Exists(tempTxtPath) && i < 9) {
                //					tempTxtPath = txtPath + "_temp" + i.ToString() + ".txt";
                //					i++;
                //				}
                //				if (File.Exists(tempTxtPath))
                //					throw new Exception("Error creating txt backup file");
                //				if (File.Exists(txtPath))
                //					File.Move(txtPath, tempTxtPath);
                //				if (File.Exists(txtPath))
                //					throw new Exception("Error creating txt backup file");
                //

                t = new Transaction(projectDoc, "CompactFamily");
                t.Start();
                //if (!projectDoc.LoadFamily(familyPath, new FamilyLoadOptions(), out loadedFamily)) throw new Exception("Error loading family");
                if (!projectDoc.LoadFamily(tempFamilyPath2, new FamilyLoadOptions(), out Family loadedFamily)) throw new Exception(rm.GetString("ErrorLoadingFamily", ci));
                t.Commit();
                Document familyDoc = projectDoc.EditFamily(loadedFamily);
                familyDoc?.Save(new SaveOptions() { Compact = true });
                familyDoc.Close(false);
                projectDoc.Close(false);
                long existingFileSize = (new FileInfo(familyPath)).Length;
                long newFileSize = (new FileInfo(tempFamilyPath2)).Length;
                long newSaveAsFileSize = (new FileInfo(tempFamilyPath1)).Length;
                long profit1 = existingFileSize - newSaveAsFileSize;
                long profit2 = existingFileSize - newFileSize;
                profit = Math.Max(profit1, profit2);
                if (profit > 0)
                {
                    string backupCopyFilePath = GetNextBackupFilePath(familyPath);
                    if (string.IsNullOrEmpty(backupCopyFilePath)) throw new Exception(rm.GetString("ErrorCreatingBackupFile", ci));
                    File.Move(familyPath, backupCopyFilePath);
                    if (profit1 >= profit2)
                    {
                        File.Move(tempFamilyPath1, familyPath);
                        File.Delete(tempFamilyPath2);
                    }
                    else
                    {
                        File.Move(tempFamilyPath2, familyPath);
                        File.Delete(tempFamilyPath1);
                    }
                    result = true;
                }
                else
                {
                    File.Delete(tempFamilyPath1);
                    File.Delete(tempFamilyPath2);
                    result = false;
                }
            }
            catch (Exception ex)
            {
                if (projectDoc != null)
                {
                    if (t != null && t.GetStatus() == TransactionStatus.Started)
                        t.RollBack();
                    try
                    {
                        projectDoc.Close(false);
                    }
                    catch (Exception ex1)
                    {
                        TaskDialog.Show("CompactFamily.catch.projectDoc", ex1.ToString());
                    }
                }
                if (familyDocToSaveAs != null)
                {
                    if (t != null && t.GetStatus() == TransactionStatus.Started)
                        t.RollBack();
                    try
                    {
                        familyDocToSaveAs.Close(false);
                    }
                    catch (Exception ex1)
                    {
                        TaskDialog.Show("CompactFamily.catch.familyDocToSaveAs", ex1.ToString());
                    }
                }
                TaskDialog.Show("CompactFamily", ex.ToString());
            }
            finally
            {
                string tempFamilyFileBackupCopyPath1 = Path.Combine(
                    Path.GetDirectoryName(tempFamilyPath1),
                    Path.GetFileNameWithoutExtension(tempFamilyPath1) + ".0001" +
                    Path.GetExtension(tempFamilyPath1)
                    );
                if (File.Exists(tempFamilyFileBackupCopyPath1))
                    try
                    {
                        File.Delete(tempFamilyFileBackupCopyPath1);
                    }
                    catch (Exception ex1)
                    {
                        TaskDialog.Show("CompactFamily.finally.tempFamilyPath1", ex1.ToString());
                    }
                string tempFamilyFileBackupCopyPath2 = Path.Combine(
                    Path.GetDirectoryName(tempFamilyPath2),
                    Path.GetFileNameWithoutExtension(tempFamilyPath2) + ".0001" +
                    Path.GetExtension(tempFamilyPath2)
                    );
                if (File.Exists(tempFamilyFileBackupCopyPath2))
                    try
                    {
                        File.Delete(tempFamilyFileBackupCopyPath2);
                    }
                    catch (Exception ex1)
                    {
                        TaskDialog.Show("CompactFamily.finally.tempFamilyPath2", ex1.ToString());
                    }
                //				if (File.Exists(tempTxtPath) && !File.Exists(txtPath))
                //				{
                //					try {
                //						File.Move(tempTxtPath, txtPath);
                //					} catch (Exception ex1) {
                //						TaskDialog.Show("CompactFamily", "Error restoring \"" + txtPath + "\" from a backup copy.");
                //					}
                //					if (File.Exists(txtPath) && File.Exists(tempTxtPath) && FileUtils.CompareFiles(tempTxtPath, txtPath))
                //					try {
                //						File.Delete(tempTxtPath);
                //					} catch (Exception ex1) {
                //						TaskDialog.Show("CompactFamily", "Error deleting \"" + tempTxtPath + "\".");
                //					}
                //				}
            }

            return result;
        }

        string GetNextBackupFilePath(string documentFileName)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(documentFileName);
            string fileExtension = Path.GetExtension(documentFileName);
            string directory = Path.GetDirectoryName(documentFileName) ?? string.Empty;
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory)) return string.Empty;
            //IEnumerable<string> fileNames = Directory.GetFiles(directory).Select(f => Path.GetFileNameWithoutExtension(f)).Where(s => Regex.IsMatch(s, "^" + fileNameWithoutExtension)).Where(f => f.Length == fileNameWithoutExtension.Length + 5).Where(f => Regex.IsMatch(f, @"\d{4}"));//.MaxBy(f => int.Parse(f.Substring(f.Length - 4)));
            string[] allFiles = Directory.GetFiles(directory);
            if (allFiles.Length == 0) return string.Empty;
            IEnumerable<string> fileNamesMatchingDocumentName = allFiles.Select(f => Path.GetFileNameWithoutExtension(f)).Where(s => Regex.IsMatch(s, "^" + fileNameWithoutExtension));
            if (!fileNamesMatchingDocumentName.Any()) return string.Empty;
            IEnumerable<string> fileNamesMatchingDocumentNameAndLength = fileNamesMatchingDocumentName.Where(f => f.Length == fileNameWithoutExtension.Length + 5);
            string result = string.Empty;
            if (!fileNamesMatchingDocumentNameAndLength.Any())
            {
                if (fileNamesMatchingDocumentName.Count() > 0)
                {
                    result = fileNamesMatchingDocumentName.FirstOrDefault(f => f == fileNameWithoutExtension);
                    if (!string.IsNullOrEmpty(result))
                        result += ".0000";
                }
                else
                    return string.Empty;
            }
            else
            {
                IEnumerable<string> fileNamesMatchingDocumentNameAndLengthEndingWith0000 = fileNamesMatchingDocumentNameAndLength.Where(f => Regex.IsMatch(f, @"\d{4}"));
                result = fileNamesMatchingDocumentNameAndLengthEndingWith0000.Aggregate((i, j) => int.Parse(i.Substring(i.Length - 4)) > int.Parse(j.Substring(j.Length - 4)) ? i : j); //.MaxBy(f => int.Parse(f.Substring(f.Length - 4))) ?? string.Empty;
            }
            if (!string.IsNullOrEmpty(result))
            {
                int nextNumber = int.Parse(result.Substring(result.Length - 4)) + 1;
                result = result.Substring(0, result.Length - 4) + nextNumber.ToString("0000");
            }
            result = Path.Combine(directory, result + fileExtension);
            if (File.Exists(result))
                throw new Exception(rm.GetString("ErrorCreatingBackupFile", ci));
            return result;
        }

        private class FamilyLoadOptions : IFamilyLoadOptions
        {
            public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
            {
                overwriteParameterValues = true;
                return true;
            }
            public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
            {
                source = FamilySource.Family;
                overwriteParameterValues = true;
                return true;
            }
        }


    }
}
