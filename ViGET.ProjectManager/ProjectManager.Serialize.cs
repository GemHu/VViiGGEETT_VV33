using Dothan.Helpers;
using Dothan.ViObject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DothanTech.ViGET.Manager
{
    public partial class ProjectManager : IViModel
    {
        public string Include
        {
            get
            {
                if (!this.IsLoading)
                {
                    return this.ViPath;
                }

                return this._include;
            }
            protected set
            {
                this._include = value;
            }
        }
        private String _include;


    }
}
