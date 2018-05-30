using DothanTech.ViGET.ViService;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DothanTech.ViGET.Shell
{
    public partial class ViShell
    {
        #region ShellManager Property

        [Import(typeof(IViShellService))]
        protected ViShellManager ShellService { get; set; }

        private void OnShellClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //this.SaveLayout();
            if (this.ShellService != null)
            {
                this.ShellService.RaiseClosing(this, e);
            }
        }

        #endregion

        #region DocManager Property

        public static readonly DependencyProperty DocManagerProperty =
            DependencyProperty.Register("DocManager", typeof(ViDocManager), typeof(ViShell),
                                        new PropertyMetadata(null));

        [Import(typeof(IViDocManager))]
        public ViDocManager DocManager
        {
            get { return (ViDocManager)GetValue(DocManagerProperty); }
            set { SetValue(DocManagerProperty, value); }
        }

        #endregion
    }
}
