using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;

namespace TypeScriptDefinitionGenerator.Tests
{
    public class SolutionWorker
    {
        private const string PhysicalFolder_guid = "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}";

        private static IEnumerable<DTE2> GetVisualStudioInstances()
        {
            IRunningObjectTable rot;
            IEnumMoniker enumMoniker;
            int retVal = GetRunningObjectTable(0, out rot);

            if (retVal == 0)
            {
                rot.EnumRunning(out enumMoniker);

                IntPtr fetched = IntPtr.Zero;
                IMoniker[] moniker = new IMoniker[1];
                while (enumMoniker.Next(1, moniker, fetched) == 0)
                {
                    IBindCtx bindCtx;
                    CreateBindCtx(0, out bindCtx);
                    string displayName;
                    moniker[0].GetDisplayName(bindCtx, null, out displayName);
                    Console.WriteLine("Display Name: {0}", displayName);
                    bool isVisualStudio = displayName.StartsWith("!VisualStudio");
                    if (isVisualStudio)
                    {
                        object obj;
                        rot.GetObject(moniker[0], out obj);
                        var dte = obj as DTE2;
                        yield return dte;
                    }
                }
            }
        }

        internal static DTE2 GetVisualStudioInstanceOfTypeScriptDefinitionGenerator()
        {
            int retryCount = 5;
            do
            {
                try
                {
                    return GetVisualStudioInstanceOfTypeScriptDefinitionGeneratorInternal();
                }
                catch (System.Runtime.InteropServices.COMException ex) when ((uint)ex.HResult == 0x8001010A)
                {
                    //System.Runtime.InteropServices.COMException: 'The message filter indicated that the application is busy. (Exception from HRESULT: 0x8001010A (RPC_E_SERVERCALL_RETRYLATER))'
                    retryCount--;
                    if (retryCount == 0) { throw new InvalidOperationException("ExamineSolution failed"); }
                    System.Threading.Thread.Sleep(1000);
                }
            } while (retryCount > 0);
            throw new Exception("unreachable code");
        }
        internal static DTE2 GetVisualStudioInstanceOfTypeScriptDefinitionGeneratorInternal()
        {
            var allDte2s = GetVisualStudioInstances();
            foreach (var dte2 in allDte2s)
            {
                if (dte2.Solution.FullName.EndsWith("TypeScriptDefinitionGenerator.sln"))
                {
                    return dte2;
                }
            }
            throw new Exception("VisualStudio instance of TypeScriptDefinitionGenerator not found");
        }

        [DllImport("ole32.dll")]
        private static extern void CreateBindCtx(int reserved, out IBindCtx ppbc);

        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

        /// <summary>
        /// Recursively finds first item in solution matching <paramref name="filename"/>.
        /// </summary>
        /// <param name="solution">solution</param>
        /// <param name="filename">wanted filename</param>
        /// <returns>ProjectItem or null</returns>
        public ProjectItem GetProjectItem(Solution solution, string filename)
        {
            ProjectItem ret = null;
            // get all the projects
            foreach (Project project in solution.Projects)
            {
                var foundFile = FindRecursive(filename, project.ProjectItems);
                if (foundFile != null)
                {
                    return foundFile;
                }
            }

            return ret;
        }

        private ProjectItem FindRecursive(string filename, ProjectItems projectItems)
        {
            // get all the items in each project
            foreach (ProjectItem item in projectItems)
            {
                if (item.Kind == PhysicalFolder_guid)
                {
                    var subProjectItems = item.ProjectItems as ProjectItems;
                    if (subProjectItems != null)
                    {
                        var foundFile = FindRecursive(filename, subProjectItems);
                        if (foundFile != null)
                        {
                            return foundFile;
                        }
                    }
                }

                // find this file and examine it
                if (item.Name == filename)
                {
                    return item;
                }
            }

            return null;
        }

        public void ExamineSolution(Solution solution)
        {
            int retryCount = 5;
            do
            {
                try
                {
                    ExamineSolutionInternal(solution);
                    retryCount = 0;
                }
                catch (System.Runtime.InteropServices.COMException ex) when ((uint)ex.HResult == 0x8001010A)
                {
                    //System.Runtime.InteropServices.COMException: 'The message filter indicated that the application is busy. (Exception from HRESULT: 0x8001010A (RPC_E_SERVERCALL_RETRYLATER))'
                    retryCount--;
                    if (retryCount == 0) { throw new InvalidOperationException("ExamineSolution failed"); }
                    System.Threading.Thread.Sleep(1000);
                }
                catch (Exception ex) {
                    throw new Exception("wrap", ex);
                }
            } while (retryCount > 0);
        }
        private void ExamineSolutionInternal(Solution solution)
        {
            Console.WriteLine(solution.FullName + " (" + solution.Projects.Count + ")");

            // get all the projects
            foreach (Project project in solution.Projects)
            {
                Console.WriteLine("\t{1}:{2}:{3}:{4}:{5}::::{0}", project.FullName,
                    project.Kind,
                    project.CodeModel,
                    "",
                    project.Name,
                    project.ProjectItems.Count);

                // get all the items in each project
                foreach (ProjectItem item in project.ProjectItems)
                {
                    //Console.WriteLine("\t\tProjectItem:{1}: {0}", item.Name, item.ExtenderNames.GetType());
                    Console.WriteLine("\t\tProjectItem: {0}", item.Name);

                    // find this file and examine it "HowToUseCodeModelSpike"
                    if (item.Name == "Constants.cs")
                    {
                        ExamineItem(item);
                    }
                }
            }
        }

        // examine an item
        private void ExamineItem(ProjectItem item)
        {
            FileCodeModel2 model = (FileCodeModel2)item.FileCodeModel;
            foreach (CodeElement codeElement in model.CodeElements)
            {
                ExamineCodeElement(codeElement, 3);
            }
        }

        // recursively examine code elements
        private void ExamineCodeElement(CodeElement codeElement, int tabs)
        {
            tabs++;
            try
            {
                Console.WriteLine(new string('\t', tabs) + "{0} {1}", codeElement.Name, codeElement.Kind.ToString());

                // if this is a namespace, add a class to it.
                if (codeElement.Kind == vsCMElement.vsCMElementNamespace)
                {
                    //AddClassToNamespace((CodeNamespace)codeElement);
                }

                foreach (CodeElement childElement in codeElement.Children)
                {
                    ExamineCodeElement(childElement, tabs);
                }
            }
            catch
            {
                Console.WriteLine(new string('\t', tabs) + "codeElement without name: {0}", codeElement.Kind.ToString());
            }
        }

        // add a class to the given namespace
        private void AddClassToNamespace(CodeNamespace ns)
        {
            // add a class
            CodeClass2 chess = (CodeClass2)ns.AddClass("Chess", -1, null, null, vsCMAccess.vsCMAccessPublic);

            // add a function with a parameter and a comment
            CodeFunction2 move = (CodeFunction2)chess.AddFunction("Move", vsCMFunction.vsCMFunctionFunction, "int", -1, vsCMAccess.vsCMAccessPublic, null);
            move.AddParameter("IsOK", "bool", -1);
            move.Comment = "This is the move function";

            // add some text to the body of the function
            EditPoint2 editPoint = (EditPoint2)move.GetStartPoint(vsCMPart.vsCMPartBody).CreateEditPoint();
            editPoint.Indent(null, 0);
            editPoint.Insert("int a = 1;");
            editPoint.InsertNewLine(1);
            editPoint.Indent(null, 3);
            editPoint.Insert("int b = 3;");
            editPoint.InsertNewLine(2);
            editPoint.Indent(null, 3);
            editPoint.Insert("return a + b; //");
        }

    }
}
