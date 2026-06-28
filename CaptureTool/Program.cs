using System; using System.Drawing; using System.Drawing.Imaging;
using System.Runtime.InteropServices; using System.Windows.Forms; using System.IO; using System.Text;
class Program {
    [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr h, out RECT r);
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll", CharSet=CharSet.Auto)] static extern int GetWindowText(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll", CharSet=CharSet.Auto)] static extern int GetClassName(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll")] static extern bool EnumWindows(EnumWindowsProc f, IntPtr p);
    delegate bool EnumWindowsProc(IntPtr h, IntPtr p);
    struct RECT { public int L,T,R,B; }
    static void Main() {
        IntPtr hwnd = IntPtr.Zero;
        EnumWindows((h,p)=>{ var t=new StringBuilder(256); var c=new StringBuilder(256);
            GetWindowText(h,t,256); GetClassName(h,c,256);
            if(!string.IsNullOrEmpty(t.ToString().Trim())&&c.ToString().Contains("WindowsForms")){hwnd=h;} return true; }, IntPtr.Zero);
        if(hwnd!=IntPtr.Zero){
            SetForegroundWindow(hwnd); System.Threading.Thread.Sleep(500);
            GetWindowRect(hwnd, out RECT r); int w=r.R-r.L, h=r.B-r.T;
            using(var bmp=new Bitmap(w,h))using(var g=Graphics.FromImage(bmp)){g.CopyFromScreen(r.L,r.T,0,0,new Size(w,h));
            bmp.Save(@"C:\Users\X2006\Desktop\Windows期末作业\screenshot_app.png");}
            Console.WriteLine($"App screenshot: {w}x{h}");
        }
    }
}
