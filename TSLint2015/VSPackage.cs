using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using TSLint;

namespace TSLint2015
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(VSPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    public sealed class VSPackage : Package
    {
        public const string PackageGuidString = "3ffa04f5-7120-4988-88f0-0c566f197e35";

        /// <summary>
        /// Initializes a new instance of the <see cref="VSPackage"/> class.
        /// </summary>
        public VSPackage()
        {
            // Void
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            // Get directory of the currently opened solution.
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));

            if (dte2 != null)
            {
                // Init linter.
                TsLint.Init(dte2);

                // Init Error List helper.
                ErrorListHelper.Init(this);
            }

            base.Initialize();
        }

        #endregion
    }
}
