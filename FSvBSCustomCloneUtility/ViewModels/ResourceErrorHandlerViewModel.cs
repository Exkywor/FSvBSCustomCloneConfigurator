// Code adaptded from
// https://github.com/ME3Tweaks/LegendaryExplorer/blob/Beta/LegendaryExplorer/LegendaryExplorer/Dialogs/ExceptionHandlerDialog.xaml.cs

using Caliburn.Micro;
using FSvBSCustomCloneUtility.Enums;
using FSvBSCustomCloneUtility.Tools;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FSvBSCustomCloneUtility.ViewModels {
    class ResourceErrorHandlerViewModel : Screen {
        public string WindowTitle { get; set; }
        public string WindowHeight { get; set; }
        public string ErrDescription { get; set; }
        public string ErrInstructions { get; set; }
        public string Errors { get; set; }
        public string ErrIcon { get; set; }
        public string ErrHeight { get; set; }

        /// <summary>
        /// Instantiate the ResourceErrorHandler window
        /// </summary>
        /// <param name="title">Window title</param>
        /// <param name="errors">List of errors to display</param>
        /// <param name="header">Header message</param>
        /// <param name="footer">Footer message</param>
        /// <param name="errFile">Path to error file</param>
        public ResourceErrorHandlerViewModel(ResourceError errType, Gender gender, string errors) {
            WindowTitle = errType.IsDuplicates() ? "Duplicate resources" : "Resources not found";
            Errors = errors;

            ErrDescription = errType.IsDuplicates()
                ? $"The morph was not applied. The following textures/hair were found in more than one mod for the {(gender.IsMale() ? "male" : "female")} headmorph."
                : 
                  $"The morph was not applied. The following textures/hair could not be found for the {(gender.IsMale() ? "male" : "female")} headmorph.";

            ErrInstructions = errType.IsDuplicates()
                ? $"Make sure to only have one mod containing the resource." +
                    Environment.NewLine +
                  $"You can disable conflicting mods while using this tool and enable them afterwards."
                : $"Make sure that any modded texture/hairs are installed, and that the names are spelled correctly in the headmorph file." +
                    Environment.NewLine +
                  $"If you cannot install the modded resources, you can remove the lines from the headmorph file.";

            ErrIcon = errType.IsDuplicates() ? "Solid_Copy" : "Solid_FileExcel";

            Size errSize = Misc.MeasureString(Errors, "Consolas", 12);
            ErrHeight = Math.Min(400, errSize.Height + 100).ToString();
            WindowHeight = (Convert.ToDouble(ErrHeight) + 160).ToString();
        }

        public void OK() {
            TryCloseAsync();
        }

        public void Copy() {
            Clipboard.SetText(Errors);
        }
    }
}
