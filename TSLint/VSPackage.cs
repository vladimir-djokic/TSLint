using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using System.IO;

namespace TSLint
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(VSPackage.PackageGuid)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    public sealed class VSPackage : Package
    {
        public const string PackageGuid = "ef75cbe5-865e-469a-860b-03678057fc36";

        public VSPackage()
        {
            // Void
        }

        #region Package Members

        protected override void Initialize()
        {
            // Get directory of the currently opened solution.
            var dte = (DTE)GetService(typeof(DTE));
            var solutionDir = Path.GetDirectoryName(dte.Solution.FullName);

            // Init linter.
            TsLint.Init(solutionDir);

            // Init Error List helper.
            ErrorListHelper.Init(this);

            base.Initialize();
        }

        #endregion
    }
}
