using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.IO;
using System.Drawing;

namespace Daemonized.TurnOut
{
    public class Browser
    {
        private FileInfo _fiPath;
        private Bitmap _imgIcon;
        private string _sArgs;

        public FileInfo Path
        {
            get { return _fiPath; }
            set { _fiPath = value; }
        }
        public Bitmap Icon
        {
            get
            {
                return _imgIcon;
            }
            set { _imgIcon = value; }
        }
        public string Arguments
        {
            get { return _sArgs; }
            set { _sArgs = value; }
        }

        public Browser(FileInfo browserPath)
        {
            if (null != _fiPath && _fiPath.Exists)
                _fiPath = browserPath;
            else
                throw new FileNotFoundException();
        }
        public Browser(string browserPath)
        {
            _fiPath = new FileInfo(browserPath);
        }
        public Browser(FileInfo browserPath, Bitmap iconFile)
            : this(browserPath)
        {
            _imgIcon = iconFile;
        }
        public Browser(string browserPath, Bitmap iconFile)
            : this(browserPath)
        {
            _imgIcon = iconFile;
        }
    }
}
