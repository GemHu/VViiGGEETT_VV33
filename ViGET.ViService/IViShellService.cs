using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DothanTech.ViGET.ViService
{
    public interface IViShellService
    {
        bool RegistorEditorFactory(String extention, Type type);

        List<String> GetRecentProjects();

        void AddRecentProject(String projectFile);

        void NewProject();

        bool OpenProject(String projectFile);

        event CancelEventHandler Closing;
    }
}
