

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using wclCommon;
using wclBluetooth;
using System.Drawing;
using System.Threading;
using System.ComponentModel;

namespace Gatt_Client_ClassLibrary
{
    //public delegate string Client_OnCharacteristicChanged(object Sender, ushort Handle, byte[] Value);
    //public delegate string wclBluetoooth.wclGattCharacteristicChangedEvent(object Sender, ushort Handle, byte[] Value);

    //public delegate string del1(object Sender, ushort Handle, byte[] Value);




    public /*partial*/ class Class1 : Form
    {

        public delegate string mydelegate(object Sender, ushort Handle, byte[] Value);//declare a delegate
        public event mydelegate del1;//del1 is event of the delegate

        //public event EventHandler Client_OnCharacteristicChanged;

        public ReaderWriterLock rwl = new ReaderWriterLock();

        public wclBluetoothManager Manager;
        //private wclGattClient Client;
        public wclGattClient[] Client_n;
        public String New_str = "";
        public String[] st;//for "Node"
        public String[] st1;//for "SN"
        public String[] sadd1;
        public String[] tadd;
        public String madd1;

        public bool serial_Click = false, scale_check = false;
        public bool[] Acquire_click = new bool[18];
        //TextBox[] textBoxes = new TextBox[15];



        public wclGattCharacteristic[] FCharacteristics;
        //private wclGattDescriptor[] FDescriptors;
        public wclGattService[] FServices;

        public BackgroundWorker[] backgroundservices;
        public System.Windows.Forms.Timer[] timers;
        public int[] discn;


        static int flag = 0;
        static int Servicecount = 0;
        public int d_count = 0;
        public int prev_d_count = 0;// in case one or more scales turn off
        public int k_index;//device index in return_str function()
        public int devices = 0;//this increments if it goes into OnConnect event
        public int scale_count = 0;//this value increases by 1 everytime we write to a device characterstic
        public int signal = 0;//indicate it has enter characterstic chaned event handler
        public /*static*/ int i = 0;
        public int no_service = 0;//if unable to find services of BLE device
        public int connected = 1;
        public int log = 0;//counts for reading services and characterstics
        public int temp;//for locally storing Client "index" when scale turns off
        public int turn_off = 0;//if a device is turned off in middle of weighing
        public int Instance = 1;//discovering is complete
        public int[] found;//if scale is found after being lost
        public int total = 0;//count for scales when re-connected
        public int fault = 0;
        public int notify = 0;//to prevent redundant error display in btsub and btsubW
        public bool done = false;



        static int d_index = -1;
        static int k1 = 0, k2 = 0;//k1 is for acq_click fn iteration
        static int Init = 0;
        static int finish = 0;


        public bool complete = false;
        //extern "C" __declspec(dllexport) string Settings_page.pad;


        public int[] not_found = new int[18];

        public Dictionary<int, long> dict0 = new Dictionary<int, long>();//from string to int
        public Dictionary<int, wclBluetoothRadio> dict = new Dictionary<int, wclBluetoothRadio>();//from string to int
        public Dictionary<int, wclGattService> dict1 = new Dictionary<int, wclGattService>();//for SERVICES//from string to int
        public Dictionary<string, wclGattCharacteristic> dict2 = new Dictionary<string, wclGattCharacteristic>();//for CHARSTICS
        public ProgressBar progressBar1;
        private Label label1;
        private Label label2;
        private IContainer components;
        private PictureBox pictureBox1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;






        /// <summary>
        //
        /// </summary>
        /// <param name="num_of_scales"></param>
        public Class1()
        {
            InitializeComponent();
            /*public ProgressBar p1 = new ProgressBar();
            //ProgressBar p1 = new ProgressBar();
            this.Controls.Add(p1);
            p1.Size = new Size(100, 50);
            p1.Minimum = 0;
            p1.Maximum = 100;
            p1.Step = 1;
            p1.Location = new Point(100, 100);
            p1.Visible = true;
            p1.Show();*/
            pictureBox1.Enabled = true;
            pictureBox1.Visible = true;
            Cursor.Current = Cursors.WaitCursor;
            progressBar1.Value = 0;
            progressBar1.Show();
        }




        public int Class1_Load(int num_of_scales)
        {

            st = new String[num_of_scales];
            st1 = new String[num_of_scales];
            sadd1 = new String[num_of_scales];
            tadd = new String[num_of_scales];
            Manager = new wclBluetoothManager();
            //Client = new wclGattClient();
            Client_n = new wclGattClient[18];
            FServices = new wclGattService[18];
            FCharacteristics = new wclGattCharacteristic[18];
            backgroundservices = new BackgroundWorker[18];//always write this statement if delaring array
            timers = new System.Windows.Forms.Timer[18];
            discn = new int[18];

            found = new int[18];
            pictureBox1.Enabled = true;

            Servicecount = 0;

            for (int temp = 0; temp < 18; temp++)
            {
                Client_n[temp] = new wclGattClient();

                Client_n[temp].OnCharacteristicChanged += new wclGattCharacteristicChangedEvent(Client_OnCharacteristicChanged);//when "SUBSCRIBE" button is clicked
                Client_n[temp].OnConnect += new wclCommunication.wclClientConnectionConnectEvent(Client_OnConnect);

                FServices[temp] = new wclGattService();
                FCharacteristics[temp] = new wclGattCharacteristic();

                backgroundservices[temp] = new BackgroundWorker();
                found[temp] = new int();


                //------------timers for reconnecting to turned off scales---------//
                timers[temp] = new System.Windows.Forms.Timer();
                timers[temp].Tick += new EventHandler(timer_tick);//this event fires when specified time interval has elapsed
                timers[temp].Interval = 1500;
                //------------timers for reconnecting to turned off scales---------//
                discn[temp] = new int();
                discn[temp] = -1;

                backgroundservices[temp].DoWork += new DoWorkEventHandler(DoService);
                backgroundservices[temp].RunWorkerCompleted += new RunWorkerCompletedEventHandler(WorkCompleted);
                //Client_n[temp].ReadServices += new wclCommunication.wclClientConnectionConnectEvent(btGetServices_Click);

                //del1 += Client_OnCharacteristicChanged;
                //public event wclGattCharactersticChangedEvent del1;
                //Client_n[temp].OnCharacteristicChanged += Client_OnCharacteristicChanged;
                not_found[temp] = 0;

            }

            Manager.OnNumericComparison += new wclBluetoothNumericComparisonEvent(Manager_OnNumericComparison);
            Manager.OnPasskeyNotification += new wclBluetoothPasskeyNotificationEvent(Manager_OnPasskeyNotification);
            Manager.OnPasskeyRequest += new wclBluetoothPasskeyRequestEvent(Manager_OnPasskeyRequest);
            Manager.OnPinRequest += new wclBluetoothPinRequestEvent(Manager_OnPinRequest);
            Manager.OnDeviceFound += new wclBluetoothDeviceEvent(Manager_OnDeviceFound);
            Manager.OnDiscoveringCompleted += new wclBluetoothResultEvent(Manager_OnDiscoveringCompleted);
            Manager.OnDiscoveringStarted += new wclBluetoothEvent(Manager_OnDiscoveringStarted);

            Manager.Open();

            //cbOpFlag.SelectedIndex = 0;

            Cleanup();


            btDiscover_Click(/*btDiscover, EventArgs.Empty*/);//call it later   
            return 1;

        }

        private void Class1_Tick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void Manager_OnNumericComparison(object Sender, wclBluetoothRadio Radio, long Address, uint Number, out bool Confirm)
        {
            // Accept any pairing.
            Confirm = true;
        }

        void Manager_OnPinRequest(object Sender, wclBluetoothRadio Radio, long Address, out string Pin)
        {
            // Use '0000' as PIN.
            Pin = "0000";
            //TraceEvent(Address, "Pin request", "PIN", Pin);
        }

        void Manager_OnPasskeyRequest(object Sender, wclBluetoothRadio Radio, long Address, out uint Passkey)
        {
            // Use 123456 as passkey.
            Passkey = 123456;
            //TraceEvent(Address, "Passkey request", "Passkey", Passkey.ToString());
        }

        void Manager_OnPasskeyNotification(object Sender, wclBluetoothRadio Radio, long Address, uint Passkey)
        {
            //TraceEvent(Address, "Passkey notification", "Passkey", Passkey.ToString());
        }

        void Manager_OnDiscoveringStarted(object Sender, wclBluetoothRadio Radio)//when "DISCOVER" button is clicked
        {
            //lvDevices.Items.Clear();
            //TraceEvent(0, "Discovering started", "", "");//TRACE EVENT IS THE LAST TABLE IN THE FORM

        }

        public void btDiscover_Click(/*object sender, EventArgs e*/)
        {
            done = false;
            wclBluetoothRadio Radio = GetRadio();
            if (Radio != null)
            {
                Int32 Res = Radio.Discover(15, wclBluetoothDiscoverKind.dkBle);
                if (Res != wclErrors.WCL_E_SUCCESS)
                    MessageBox.Show("Error starting discovering: 0x" + Res.ToString("X8"),
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public wclBluetoothRadio GetRadio()
        {
            // It is a little bit complex here:
            // As Microsoft Bluetooth driver is always available in the system
            // there can be 2 radios retruned if other than MS driver also installed.
            // This rountine checks that and returns second radio in such case.
            if (Manager.Count == 1)
                // Only MS Radio is available.
                return Manager[0];

            // Other driver also installed.
            for (Int32 i = 0; i < Manager.Count; i++)
                if (Manager[i].Api != wclBluetoothApi.baMicrosoft)
                    // Return first non MS.
                    return Manager[i];

            MessageBox.Show("No one Bluetooth Radio found.", "Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return null;
        }

        public void Manager_OnDeviceFound(object Sender, wclBluetoothRadio Radio, long Address)//control comes here whenever a new device is found
        {
            String DevName;
            wclBluetoothDeviceType DevType = wclBluetoothDeviceType.dtBle;
            Int32 Res = Radio.GetRemoteDeviceType(Address, out DevType);
            //Radio.GetName(out DevName);
            Radio.GetRemoteName(Address, out DevName);



            if (turn_off == 0)
            {
                if (Address.ToString("X12").Substring(0, 4) == "D0CF" && Res == wclErrors.WCL_E_SUCCESS && DevType.ToString() == "dtBle")//filters only node-1
                {
                    dict0.Add(d_count, Address);//dict0 stores addresses
                    dict.Add(d_count, Radio);//used by tag1 to "connect to client" later on
                    d_count++;
                }

                progressBar1.Increment(d_count);
                label1.Text = d_count + " device(s) found...";
            }


            if (turn_off == 1 && Address.ToString("X12").Substring(0, 4) == "D0CF" && Res == wclErrors.WCL_E_SUCCESS && DevType.ToString() == "dtBle")
            {
                try
                {
                    dict.Add(discn[total], Radio);//used by tag1 to "connect to client" later on
                }
                catch (IndexOutOfRangeException e)
                {
                    MessageBox.Show("Please Click Again");
                }
                catch (SystemException e)
                {
                    MessageBox.Show("Same Directory key.Please Click Again");
                }
                try
                {
                    found[discn[total]] = 1;
                    not_found[discn[total]] = 0;
                }
                catch (IndexOutOfRangeException e)
                {
                    MessageBox.Show("Please Click Again");
                }
                discn[total] = -1;//restore default values of discn[]
                total++;
                //turn_off = 0;
            }
        }

        public async void Manager_OnDiscoveringCompleted(object Sender, wclBluetoothRadio Radio, int Error)//
        {
            total = 0;//reset total value for reconnecting again if done
            Instance = 1;//to prevent discovery process to start once again
            if (turn_off == 0)
            {
                if (d_count > 0)
                {
                    while (i < d_count)//try to traverse all the bluetooth devices
                    {

                        /*Task<int> devices =*/
                        btConnect_Click();
                        //Connect.PerformClick();
                        await Task.Delay(1000);//means that Manager_OnDiscoveringCompleted cant run unless 1400ms are over
                                               //give it some time to actually establish connection


                        //await btConnect_Click();
                        i++;

                    }
                }



                label1.Text = "Discovery completed!";
                progressBar1.Value = 10;
                //await Task.Delay(20);

                this.WindowState = FormWindowState.Minimized;
                //pictureBox1.Enabled = false;

                //complete = true;--previously here


                //ReadServices();
                //Readcharacterstics();
                /*while (log < d_count)
                {*/
                   
                    //await Task.Delay(400);
                    btGetServices_Click();
                    btGetCharacteristics_Click();
                    log++;

                /*}
                log = 0;*/
            }
            //progressBar1.Hide();

            if (d_count == 0)
                MessageBox.Show("No Devices Found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //else
            //MessageBox.Show("Ready to Acquire!");

            Cursor.Current = Cursors.Default;
            done = true;//discover is done
            complete = true;


        }

        //if a device turns off in between try reconnecting to it
        private void DoService(object sender, DoWorkEventArgs e)
        {
            btGetServices_Click();
            btGetCharacteristics_Click();
        }

        private void WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }


        public async void timer_tick(Object myObject, EventArgs myEventArgs)//timer for all those scales which are re-connecting
        {
            int Res = 1;
            wclGattService Localservice;
            wclBluetoothRadio tag;
            if (Res != 0)
            {
                dict.TryGetValue(temp, out tag);
                Res = Client_n[temp].Connect(tag);
                await Task.Delay(1400);
                Res = Client_n[temp].ReadServices(wclGattOperationFlag.goNone, out FServices);
                dict1.TryGetValue(0, out Localservice);
                Res = Client_n[temp].ReadCharacteristics(Localservice, wclGattOperationFlag.goNone, out FCharacteristics);
            }
            if (Res == 0)
            {
                timers[temp].Enabled = false;
            }

        }

        public void Servicestatus()
        {
            label2.Text = "Finding services for " + (log + 1) + "th device";
        }

        public async void ReadServices()
        {
            log = 0;
            int found = 0;
            while (log < d_count)
            {
                await Task.Delay(500);
                //Svice.PerformClick();
                if (no_service == 1)
                {
                    no_service = 0;
                    continue;
                }
                //found = btGetServices_Click();
                //if(found == 1)
                log++;
            }
        }

        public void Readcharacterstics()
        {
            log = 0;
            while (log < d_count)
            {
                //Chrstic.PerformClick();
                btGetCharacteristics_Click();
                log++;
            }
            log = 0;
        }

        private void Cleanup()
        {
            FCharacteristics = null;
            //FDescriptors = null;
            FServices = null;
        }

        public void Remove_connection()
        {
            Manager.Close();
            Manager = null;

            // wclBluetoothRadio Radio = GetRadio();
            //Radio.Terminate();
            for (int temp = 0; temp < 18; temp++)
            {
                Client_n[temp].Disconnect();
                Client_n[temp] = null;
            }
            Cleanup();
        }


        private wclGattOperationFlag OpFlag()
        {
            switch (0)
            {
                case 1:
                    return wclGattOperationFlag.goReadFromDevice;
                case 2:
                    return wclGattOperationFlag.goReadFromCache;
                default:
                    return wclGattOperationFlag.goNone;
            }
        }

        public async /*Task<int>*/void btConnect_Click()
        {
            wclBluetoothRadio tag1;
            long address;
            await Task.Delay(1);
            //if (dict.TryGetValue(0,out tag1))
            if (dict.TryGetValue(i, out tag1))
            {
                //if (dict0.TryGetValue(0, out address))
                if (dict0.TryGetValue(i, out address))
                {
                    String DevName;
                    //tag1.GetRemoteName(address, out DevName);
                    //d_index = (int.Parse(DevName.Substring(5, 1))) - 1;
                    d_index = (int.Parse(address.ToString("X12").Substring(10, 2))) - 1;
                    Client_n[d_index].Address = address;
                    //Client_n.Address = address;
                }
                //Client_n.Connect(tag1);//client connected  
                Client_n[d_index].Connect(tag1);//client connected
            }
            //return devices;

        }


        public /*async*/ void/*int*//*Task<int>*/ btGetServices_Click(/*object sender,EventArgs e*/)//"GET SERVICES" button
        {
            Int32 Res;
            //Servicestatus();
            //await Task.Delay(100);--now
            
            FServices = null;//wclgatservice[] type ka variable is FServices
            

             

            //rwl.AcquireReaderLock(Timeout.Infinite);
            //try
            {
                Res = Client_n[0].ReadServices(OpFlag(), out FServices);
            }

            //finally
            //{
                //rwl.ReleaseReaderLock();
            //}
            if (Res != wclErrors.WCL_E_SUCCESS)
            {
                //MessageBox.Show("Error: 0x" + (Client_n[log].ReadServices(wclGattOperationFlag.goNone, out FServices)).ToString("X8") + "for the" + i + "th device(services)!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                no_service = 2;
                return;
            }

            if (FServices == null)
            {
                MessageBox.Show("No Services Found for the " + (0 + 1) + "th device");
                no_service = 1;
                return;
            }

            foreach (wclGattService Service in FServices)//enumerate the 2nd table with service details
            {


                String s;

                //*********************2nd TABLE IN THE FORM "SERVICES"******************
                if (Service.Uuid.IsShortUuid)
                    s = Service.Uuid.ShortUuid.ToString("X4");//1st col
                else
                    s = Service.Uuid.LongUuid.ToString();////1st col


                Servicecount++;
                if (Servicecount == 7)//it will go inside if statement only the first time
                {//because its the last service added in GATT editor
                    dict1.Add(0, Service);//take the last service used to find the two chracterstics
                                            //Servicecount = 0;
                }
                //dict1.Add("Node-1", Service);//take the last service used to find the two chracterstics

            }

            //MessageBox.Show("Services Found for the " + (log + 1) + "th device");

            return;
        }


        public void btGetCharacteristics_Click(/*object sender, EventArgs e*/)//when "GET CHARACTERSTICS BUTTON" IS CLICKED
        {

            //MessageBox.Show("Ready to Acquire!");//-->just put after chrstics
            //await Task.Delay(250);


            FCharacteristics = null;
            wclGattService Localservice;

            if (dict1.TryGetValue(0, out Localservice))//it will always take chrstics of 0th device
            {

                //while ((Client_n[i].ReadCharacteristics(Localservice, wclGattOperationFlag.goNone, out FCharacteristics)) != wclErrors.WCL_E_SUCCESS) ;//read the characterstics from device or cache as told by opflag


                if ((Client_n[0].ReadCharacteristics(Localservice, wclGattOperationFlag.goNone, out FCharacteristics)) != wclErrors.WCL_E_SUCCESS)
                {
                    MessageBox.Show("Error: 0x" + (Client_n[0].ReadCharacteristics(Localservice, wclGattOperationFlag.goNone, out FCharacteristics)).ToString("X8") + "for the" + i + "th device(chrstic)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }


            if (FCharacteristics != null)
            {

                foreach (wclGattCharacteristic Character in FCharacteristics)
                {
                    String s;
                    if (Character.Uuid.IsShortUuid)
                        s = Character.Uuid.ShortUuid.ToString("X4");
                    else
                        s = Character.Uuid.LongUuid.ToString();
                    //ListViewItem Item = lvCharacteristics.Items.Add(s);//lvCharacteristics IS THE NAME OF THE LISTBOX OF CHARACTERSTICS

                    dict2.Add(s, Character);//dict2 stores UUIDs and characterstics

                }
            }

            //Acq_click();

        }

        void Client_OnConnect(object Sender, int Error)
        {
            label1.Text = "Node" + (d_index + 1) + " device connected!";
            label2.Text = "Node" + (d_index + 1) + " Error: " + Error.ToString("X8");
            /*if (Error.ToString("X8") != "00000000")
            {
                MessageBox.Show("Connection error for" + i + "th device");
            }*/
            
            devices++;

        }





        String checksum(string str)
        {
            int sum = 0, sum1, ans1, ans2;
            string temp;
            double diff;
            char cans1, cans2;
            for (int x = 0; x < str.Length; x++)
            {
                sum += Convert.ToInt32(str[x]);
            }
            sum1 = sum % 256;
            diff = (sum1 / 16) - (int)(sum1 / 16);
            if (diff < 0)
            {
                ans1 = ((int)(sum1 / 16) - 1) + 64;
            }
            else
            {
                ans1 = (int)(sum1 / 16) + 64;
            }

            ans2 = (sum1 % 16) + 64;
            cans1 = Convert.ToChar(ans1);
            cans2 = Convert.ToChar(ans2);
            temp = str + cans1 + cans2 + "!";
            return temp;

        }


        bool validate(string Temp)
        {
            int sum = 0, sum1, ans1, ans2, cmd_index = -1;
            double diff;
            string temp;
            char cans1, cans2;
            string scheck;
            char check1, check2;

            int startlength = Temp.IndexOf("$");
            if (Temp.Substring(1, 1) == "X")
            {
                cmd_index = Temp.IndexOf("X");

            }

            else if (Temp.Substring(1, 1) == "I"/*serial_Click == true*/)
            {
                cmd_index = Temp.IndexOf("I");

            }


            if (startlength != -1 && cmd_index == 1)
            {
                if (Temp.Length > 7)
                {
                    int endlength = Temp.IndexOf("!");
                    if (endlength != -1 && endlength > startlength)
                    {
                        scheck = Temp.Substring(startlength, (endlength - startlength) - 2);// '$X0020+123456+GF123456+123456+123456
                        check1 = Temp[endlength - 2];//  'C
                        check2 = Temp[endlength - 1];//  '@
                        int a = scheck.Length;
                        char[] mine = new char[10];
                        mine = scheck.ToCharArray();
                        for (int t = 0; t < a; t++)
                            sum += Convert.ToInt32(mine[t]);

                        sum1 = sum % 256;
                        diff = (sum1 / 16) - (int)(sum1 / 16);

                        if (diff < 0)
                        {
                            ans1 = ((int)(sum1 / 16) - 1) + 64;
                        }
                        else
                        {
                            ans1 = (int)(sum1 / 16) + 64;
                        }

                        ans2 = (sum1 % 16) + 64;
                        cans1 = Convert.ToChar(ans1);
                        cans2 = Convert.ToChar(ans2);
                        if (cans1 == check1 && cans2 == check2)
                            return true;
                        else
                            return false;
                    }
                    return false;//if endlength is not -1
                }
                return false;//if total lenght  < 7 
            }
            return false;//if $ is not a t 1st position
        }




        public void return_str(String str, String madd, String sadd)
        {
            //int k_index;
            k_index = int.Parse(sadd) - 1;
            if (serial_Click == true)
                st1[k_index] = str;
            else
                st[k_index] = str;

            madd1 = madd;
            sadd1[k_index] = sadd;

        }

        public void Client_OnCharacteristicChanged(object Sender, ushort Handle, byte[] Value)//when indication is rcvd
        {
            int flag = 0, startindex, endindex;
            char c;
            int count1 = 0;
            String Str = " ", madd = " ", sadd = " ";
            New_str = "";
            //TraceEvent(((wclGattClient)Sender).Address, "ValueChanged", "Handle", Handle.ToString("X4"));
            if (Value == null) { }
            //TraceEvent(0, "", "Value", "");
            else if (Value.Length == 0) { }
            //TraceEvent(0, "", "Value", "");
            else
            {


                Str = Encoding.ASCII.GetString(Value, 0, Value.Length);

                //TraceEvent(0, "", "Value", output.ToString());

                //TraceEvent(0, "", "Value", Str);




                if (validate(Str))
                {
                    startindex = Str.IndexOf("$");
                    endindex = Str.IndexOf("!");
                    madd = Str.Substring(startindex + 2, 2);
                    sadd = Str.Substring(startindex + 4, 2);

                    //if (Acquire_click[k1] == true)


                    for (Int32 i = 0; i < Str.Length; i++)
                    {

                        c = (char)Str[i];
                        if (c == '+' || c == '-')
                            flag = 1;
                        if (flag == 1 && ((c >= '0' && c <= '9') || c == '-'))
                        {
                            New_str = New_str + c;

                        }
                        if (i + 1 < Str.Length)
                        {
                            if (flag == 1 && (Str[i + 1] < '0' || Str[i + 1] > '9'))//make flag 0 if the very next char is not b/w 0 and 9
                            {
                                flag = 0;
                                count1 = 1;
                                break;
                            }
                        }

                        //Str1 = Str1 + Value[i].ToString();
                        //Str = Str + Value[i].ToString("X2");
                    }
                    if (count1 == 1)
                    {
                        //charval_txt.Text = New_str + " lbs ";
                        //w1_arm_txtbx[k].Text = New_str + " lbs ";
                        count1 = 0;

                    }
                    signal = 1;//indicates we are inside this event handler
                    return_str(New_str, madd, sadd);



                    /*else*/
                    /*if (serial_Click == true)
                    {
                        New_str = Str.Substring(startindex + 7, ((endindex - 2) - (startindex + 7)));
                        return_str(New_str, madd, sadd);
                    }*/

                }



            }


            if (not_found[k_index] == 0)//k_index is returned from return_str function
            {
                Acquire_click[k_index] = false;
                btWriteUnsubsc_Click();
                btUnsubscribe_Click();
                //Unsub.PerformClick();
                //UnsubW.PerformClick();
            }

            //await Task.Delay(200);

            k2++;
            if (k2 == d_count)
            {
                //Acquire_click[k2] = false;
                //serial_Click = false;
                k2 = 0;
            }

        }


        public void btSubscribe_Click(/*object sender, EventArgs e*/)
        {
            wclGattCharacteristic charz;

            if (dict2.TryGetValue("8d164d06-a0f9-4755-9fff-a9d1a3ba3d19", out charz))
            {
                wclGattCharacteristic Characteristic = charz;////sbhi select options m 0th vale ka index kyunki select to beech se bhi kr skte h so beech wala  [0] hoga,uska index

                Int32 Res = Client_n[k1].Subscribe(Characteristic);
                if (Res != wclErrors.WCL_E_SUCCESS && Res.ToString("X8").Substring(3, 5) != "51019")
                {
                    fault = 1;
                    if (timers[k1].Enabled == false)
                        timers[k1].Enabled = true;//timer for each node

                    //if (fault == 1 /*&& notify != 1*/)
                    //{
                        notify = 1;
                        MessageBox.Show("Turn ON scale-" +(k1 + 1)+ "if it is OFF");
                    //}
                    /*else
                        MessageBox.Show("Error: 0x" + Res.ToString("X8") + " for " + (k1 + 1) + "th device!(btsub)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);*/
                }
               else if (Res.ToString("X8").Substring(3, 5) == "51009" || Res.ToString("X8").Substring(3, 5) == "50015")//attribute not found/unexpected error
                {
                    fault = 1;

                    /*if (notify != 1)
                    {
                        if (fault == 1 && notify != 1)
                        {
                            notify = 1;
                            MessageBox.Show("Turn ON scale-" +(k1 + 1)+ "if it is OFF");
                        }
                        else
                        {*/
                            MessageBox.Show((k1 + 1) + "th" + "Turned off or out of range");
                            not_found[k1] = 1;
                            sadd1[k1] = "-1";
                        //}
                    //}

                    //prev_d_count = d_count;//if one or more scale is turned off
                    //dict.Remove(k1);//Remove that entry to avoid directory index conflict
                    //btDiscover_Click();//retry finding the turned off 

                }

            }
        }

        public void btUnsubscribe_Click(/*object sender, EventArgs e*/)
        {
            wclGattCharacteristic charz;

            if (dict2.TryGetValue("8d164d06-a0f9-4755-9fff-a9d1a3ba3d19", out charz))
            {
                wclGattCharacteristic Characteristic = charz;

                Int32 Res = Client_n[k_index].Unsubscribe(Characteristic);
                //Int32 Res1 = Client2.Unsubscribe(Characteristic);
                if (Res != wclErrors.WCL_E_SUCCESS && Res.ToString("X8").Substring(3, 5) != "5101A")//not subscribed
                    MessageBox.Show("Error: 0x" + Res.ToString("X8") + "for" + (k_index + 1) + "th device!(btUnsub)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (Res.ToString("X8").Substring(3, 5) == "51009")//attribute not found
                {
                    MessageBox.Show((k_index + 1) + "th" + "Turned off or out of range");

                    //prev_d_count = d_count;//if one or more scale is turned off
                    //dict.Remove(k_index);//Remove that entry to avoid directory index conflict
                    //btDiscover_Click();//retry finding the turned off device
                }
            }
        }

        public void btWriteUnsubsc_Click(/*object sender, EventArgs e*/)//"Unsubscribe Write CCCD Subscribe Button"
        {
            wclGattCharacteristic charz;

            //wclGattCharacteristic Characteristic = FCharacteristics[lvCharacteristics.SelectedItems[0].Index];
            if (dict2.TryGetValue("8d164d06-a0f9-4755-9fff-a9d1a3ba3d19", out charz))
            {
                wclGattCharacteristic Characteristic = charz;

                Int32 Res = Client_n[k_index].WriteClientConfiguration(Characteristic, false, wclGattOperationFlag.goNone);
                if (Res != wclErrors.WCL_E_SUCCESS && Res.ToString("X8").Substring(3, 5) != "5101A")//not subscribed
                    MessageBox.Show("Error: 0x" + Res.ToString("X8") + "for" + (k_index + 1) + "th device!(btWun)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (Res.ToString("X8").Substring(3, 5) == "51009")//attribute not found
                {
                    MessageBox.Show((k_index + 1) + "th" + "Turned off or out of range");

                    //prev_d_count = d_count;//if one or more scale is turned off
                    //dict.Remove(k_index);//Remove that entry to avoid directory index conflict
                    //btDiscover_Click();//retry finding the turned off device
                }
            }

        }


        public void btWriteSubsc_Click(/*object sender, EventArgs e*/)//"Write CCCD Subscribe Button"
        {
            wclGattCharacteristic charz;
            /*if (lvCharacteristics.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select characteristic", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }*/
            if (dict2.TryGetValue("8d164d06-a0f9-4755-9fff-a9d1a3ba3d19", out charz))
            {
                wclGattCharacteristic Characteristic = charz;

                Int32 Res = Client_n[k1].WriteClientConfiguration(Characteristic, true, wclGattOperationFlag.goNone);

                if (Res != wclErrors.WCL_E_SUCCESS && Res.ToString("X8").Substring(3, 5) != "51019")
                {
                    fault = 1;
                    if (timers[k1].Enabled == false)
                        timers[k1].Enabled = true;
                    /*if (notify != 1)
                    {
                        if (fault == 1 && notify != 1)
                        {*/
                    if (notify != 1)
                    {
                        notify = 1;
                        MessageBox.Show("Turn ON scale-" + (k1 + 1) + "if it is OFF");
                    }
                        /*}
                        else
                            MessageBox.Show("Error: 0x" + Res.ToString("X8") + "for" + (k1 + 1) + "th device!(btWSub)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }*/
                }


                else if (Res.ToString("X8").Substring(3, 5) == "51009" || Res.ToString("X8").Substring(3, 5) == "50015")//Attribute not found/unexpected error
                {
                    fault = 1;
                    /*if (notify != 1)
                    {
                        if (fault == 1 && notify != 1)
                        {
                            notify = 1;
                            MessageBox.Show("Turn ON scale-" + (k1 + 1) + "if it is OFF");
                        }
                        else
                        {*/
                            MessageBox.Show((k1 + 1) + "th" + "Turned off or out of range");
                            not_found[k1] = 1;
                            sadd1[k1] = "-1";
                        //}
                    //}

                }
            }

        }

        public void Reconnect()
        {
            int mat = 0;
            fault = 1;
            /*temp = index;
            found[temp] = 0;
            turn_off = 1;//one or more scales are off
            dict.Remove(temp);//remove previous radio with which scale was connected
                              
            discn[mat++] = temp;*/

            //this section to start discovering lost scales again//-------------------
            if (fault == 1)
            {
                btDiscover_Click();
                //fault = 0;
                //Instance = 0;
            }
            for (int prev = 0; prev < 18; prev++)
            {
                if (discn[prev] != -1)
                {
                    timers[discn[prev]].Enabled = true;
                }
                //discn[prev] = -1;//restore default values of discn[]
            }
            //this section to start discovering lost scales again//-------------------
        }


            public async void Acq_click()
        {
            wclGattCharacteristic charz;
            int ascii = 70;
            int mat = 0;//count for disconnected scales;their timers

            scale_count = 0;
            scale_check = false;
            signal = 0;
            if (prev_d_count != 0)//if one or more scale is turned off
                d_count = prev_d_count;//restore previous count

            while (k1 < d_count)//acquire the chrstic,subscribe it, write CCCD
            {
                if (dict2.TryGetValue("5fe2c932-72aa-401e-a628-afc30b7a7612", out charz))
                {
                    //wclGattCharacteristic Characteristic = FCharacteristics[lvCharacteristics.SelectedItems[0].Index];
                    wclGattCharacteristic Characteristic = charz;


                    //tadd[k1] = "0" + ((k1+1)).ToString();
                    tadd[k1] = "0" + ((k1 + 1));//bug fix in future for >10
                    String Str = "$A" + tadd[k1] + "00";
                    Str = checksum(Str);
                    byte[] bytes = Encoding.ASCII.GetBytes(Str);//convert char array to byte array for  transmission
                    int j = 0;
                    while (j < 3)
                    {

                        Int32 Res = Client_n[k1].WriteCharacteristicValue(Characteristic, bytes);

                        //await Task.Delay(200);


                        //if (Client_n[k].WriteCharacteristicValue(Characteristic, bytes) != wclErrors.WCL_E_SUCCESS)
                        if (Res != wclErrors.WCL_E_SUCCESS)
                        {
                            fault = 1;
                            temp = k1;
                            found[temp] = 0;
                            turn_off = 1;//one or more scales are off
                            dict.Remove(temp);//remove previous radio with which scale was connected
                                              //dict0.Remove(k1);
                                              
                            discn[mat++] = temp;

                            break;

                            //j++;
                            //continue;


                            //MessageBox.Show("Error: 0x" + Res.ToString("X8"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            Acquire_click[k1] = true;
                            serial_Click = false;
                            scale_count++;
                            break;
                        }
                        j++;
                    }

                }
                k1++;
            }

            //this section to start discovering lost scales again//-------------------
            if (fault == 1)
            {
                btDiscover_Click();
                //fault = 0;
                //Instance = 0;
            }
            for (int prev = 0; prev < 18; prev++)
            {
                if (discn[prev] != -1)
                {
                    timers[discn[prev]].Enabled = true;
                }
                //discn[prev] = -1;//restore default values of discn[]
            }
            //this section to start discovering lost scales again//-------------------
            

            k1 = 0;
            while (k1 < d_count)
            {
                //await Task.Delay(500);
                if (not_found[k1] == 0)
                {
                    btSubscribe_Click();
                    btWriteSubsc_Click();
                    notify = 0;
                    //Sub.PerformClick();
                    //Wsub.PerformClick();
                    //await Task.Delay(500);
                }
                k1++;
            }
            notify = 0;
            if (scale_count == d_count)
            {
                scale_check = true;
            }
            k1 = 0;
            fault = 0;

            //await Task.Delay(500);

        }


        public void Zero_Click()
        {
            wclGattCharacteristic charz;
            int x = 0;

            for (x = 0; x < d_count; x++)//acquire the chrstic,subscribe it, write CCCD
            {

                if (dict2.TryGetValue("5fe2c932-72aa-401e-a628-afc30b7a7612", out charz))
                {
                    wclGattCharacteristic Characteristic = charz;
                    /*if (lvCharacteristics.SelectedItems.Count == 0)//LV is List View
                    {
                        MessageBox.Show("Select characteristic", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    wclGattCharacteristic Characteristic = FCharacteristics[lvCharacteristics.SelectedItems[0].Index];*/

                    String Str = "$D3100";
                    Str = checksum(Str);
                    byte[] bytes = Encoding.ASCII.GetBytes(Str);
                    for (int j = 0; j < 3; j++)
                    {
                        Int32 Res = Client_n[x].WriteCharacteristicValue(Characteristic, bytes);
                        if (Res != wclErrors.WCL_E_SUCCESS)
                            continue;
                        else
                            break;

                    }
                }
                //Int32 Res1 = Client2.WriteCharacteristicValue(Characteristic, Val);
                /*if (Res != wclErrors.WCL_E_SUCCESS)
                    MessageBox.Show("Error: 0x" + Res.ToString("X8"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (Res1 != wclErrors.WCL_E_SUCCESS)
                    MessageBox.Show("Error: 0x" + Res.ToString("X8"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);*/
                //click on subscribe button and Write CCCD subscribe button only ONCE!
                //Basically we are reading the 2nd charaterstic in the following two functions

                /********WE DONT WANT ZERO COMMAND TO TURN ON INDICATION************/
                //btSubscribe_Click(btSubscribe, EventArgs.Empty);
                //btWriteSubsc_Click(btWriteSubsc, EventArgs.Empty);//after enabling indications-->charstc_changed
            }

        }



        public void Ping_Click()
        {
            wclGattCharacteristic charz;
            int x = 0;

            for (x = 0; x < d_count; x++)//acquire the chrstic,subscribe it, write CCCD
            {

                if (dict2.TryGetValue("5fe2c932-72aa-401e-a628-afc30b7a7612", out charz))
                {
                    wclGattCharacteristic Characteristic = charz;
                    /*if (lvCharacteristics.SelectedItems.Count == 0)//LV is List View
                    {
                        MessageBox.Show("Select characteristic", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    wclGattCharacteristic Characteristic = FCharacteristics[lvCharacteristics.SelectedItems[0].Index];*/

                    String Str = "$E3100";
                    Str = checksum(Str);
                    byte[] bytes = Encoding.ASCII.GetBytes(Str);
                    for (int j = 0; j < 3; j++)
                    {
                        Int32 Res = Client_n[x].WriteCharacteristicValue(Characteristic, bytes);
                        if (Res != wclErrors.WCL_E_SUCCESS)
                            continue;
                        else
                            break;

                    }
                }
                //Int32 Res1 = Client2.WriteCharacteristicValue(Characteristic, Val);
                /*if (Res != wclErrors.WCL_E_SUCCESS)
                    MessageBox.Show("Error: 0x" + Res.ToString("X8"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (Res1 != wclErrors.WCL_E_SUCCESS)
                    MessageBox.Show("Error: 0x" + Res.ToString("X8"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);*/
                //click on subscribe button and Write CCCD subscribe button only ONCE!
                //Basically we are reading the 2nd charaterstic in the following two functions

                /********WE DONT WANT ZERO COMMAND TO TURN ON INDICATION************/
                //btSubscribe_Click(btSubscribe, EventArgs.Empty);
                //btWriteSubsc_Click(btWriteSubsc, EventArgs.Empty);//after enabling indications-->charstc_changed
            }

        }

        private void InitializeComponent()
        {
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(92, 204);
            this.progressBar1.Maximum = 10;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(100, 23);
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(105, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 13);
            this.label1.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(118, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 13);
            this.label2.TabIndex = 5;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Gatt_Client_ClassLibrary.Properties.Resources.loading;
            this.pictureBox1.Location = new System.Drawing.Point(61, 64);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(131, 109);
            this.pictureBox1.TabIndex = 10;
            this.pictureBox1.TabStop = false;
            // 
            // Class1
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBar1);
            this.Name = "Class1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Class1_FormClosed);
            this.Load += new System.EventHandler(this.Class1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }





        private void Connect_Click(object sender, EventArgs e)
        {
            wclBluetoothRadio tag1;
            long address;
            //if (dict.TryGetValue(0,out tag1))
            if (dict.TryGetValue(i, out tag1))
            {
                //if (dict0.TryGetValue(0, out address))
                if (dict0.TryGetValue(i, out address))
                {
                    String DevName;
                    tag1.GetRemoteName(address, out DevName);
                    d_index = (int.Parse(DevName.Substring(5, 1))) - 1;
                    Client_n[d_index].Address = address;
                    //Client_n.Address = address;
                }
                //Client_n.Connect(tag1);//client connected  
                Client_n[d_index].Connect(tag1);//client connected


            }

        }

        private void Svice_Click_1(object sender, EventArgs e)
        {
            //MessageBox.Show("connected!!");//-->just put after getchrstics

            //lvServices.Items.Clear();
            //FServices = null;//wclgatservice[] type ka variable is FServices


            Int32 res = Client_n[log].ReadServices(OpFlag(), out FServices);

            if (res != wclErrors.WCL_E_SUCCESS)
            {
                MessageBox.Show("Error: 0x" + (Client_n[log].ReadServices(wclGattOperationFlag.goNone, out FServices)).ToString("X8") + "for the" + (log + 1) + "th device(services)!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                no_service = 2;
                return;
            }

            if (FServices == null)
            {
                MessageBox.Show("No Services Found for the " + (log + 1) + "th device");
                no_service = 1;
                return;
            }

            foreach (wclGattService Service in FServices)//enumerate the 2nd table with service details
            {


                String s;

                //*********************2nd TABLE IN THE FORM "SERVICES"******************
                if (Service.Uuid.IsShortUuid)
                    s = Service.Uuid.ShortUuid.ToString("X4");//1st col
                else
                    s = Service.Uuid.LongUuid.ToString();////1st col


                Servicecount++;
                if (Servicecount == 7)//it will go inside if statement only the first time
                {//because its the last service added in GATT editor
                    dict1.Add(log, Service);//take the last service used to find the two chracterstics
                                            //Servicecount = 0;
                }                       //dict1.Add("Node-1", Service);//take the last service used to find the two chracterstics
            }

        }

        private void Chrstic_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Ready to Acquire!");//-->just put after chrstics
            //await Task.Delay(250);


            FCharacteristics = null;
            wclGattService Localservice;

            if (dict1.TryGetValue(log, out Localservice))//it will always take chrstics of 0th device
            {

                //while ((Client_n[i].ReadCharacteristics(Localservice, wclGattOperationFlag.goNone, out FCharacteristics)) != wclErrors.WCL_E_SUCCESS) ;//read the characterstics from device or cache as told by opflag


                if ((Client_n[log].ReadCharacteristics(Localservice, wclGattOperationFlag.goNone, out FCharacteristics)) != wclErrors.WCL_E_SUCCESS)
                {
                    MessageBox.Show("Error: 0x" + (Client_n[log].ReadCharacteristics(Localservice, wclGattOperationFlag.goNone, out FCharacteristics)).ToString("X8") + "for the" + (log + 1) + "th device(chrstic)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }


            if (FCharacteristics != null)
            {

                foreach (wclGattCharacteristic Character in FCharacteristics)
                {
                    String s;
                    if (Character.Uuid.IsShortUuid)
                        s = Character.Uuid.ShortUuid.ToString("X4");
                    else
                        s = Character.Uuid.LongUuid.ToString();
                    //ListViewItem Item = lvCharacteristics.Items.Add(s);//lvCharacteristics IS THE NAME OF THE LISTBOX OF CHARACTERSTICS

                    dict2.Add(s, Character);//dict2 stores UUIDs and characterstics

                }
            }

            //Acq_click();
        }

        private void Class1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Manager.Close();
            Manager = null;

            // wclBluetoothRadio Radio = GetRadio();
            //Radio.Terminate();
            for (int temp = 0; temp < 18;  temp++)
            {
                Client_n[temp].Disconnect();
                Client_n[temp] = null;
            }
            Cleanup();
        }

        private void Sub_Click(object sender, EventArgs e)
        {
            wclGattCharacteristic charz;

            if (dict2.TryGetValue("8d164d06-a0f9-4755-9fff-a9d1a3ba3d19", out charz))
            {
                wclGattCharacteristic Characteristic = charz;////sbhi select options m 0th vale ka index kyunki select to beech se bhi kr skte h so beech wala  [0] hoga,uska index

                Int32 Res = Client_n[k1].Subscribe(Characteristic);
                if (Res != wclErrors.WCL_E_SUCCESS && Res.ToString("X8").Substring(3, 5) != "51019")
                    MessageBox.Show("Error: 0x" + Res.ToString("X8") + " for " + k1 + "th device!(btsub)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (Res.ToString("X8").Substring(3, 5) == "51009" || Res.ToString("X8").Substring(3, 5) == "50015")//attribute not found/unexpected error
                {
                    MessageBox.Show(k1 + "th" + "Turned off or out of range");
                    not_found[k1] = 1;
                    sadd1[k1] = "-1";
                }

            }
        }

        private void Wsub_Click(object sender, EventArgs e)
        {
            wclGattCharacteristic charz;
            /*if (lvCharacteristics.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select characteristic", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }*/
            if (dict2.TryGetValue("8d164d06-a0f9-4755-9fff-a9d1a3ba3d19", out charz))
            {
                wclGattCharacteristic Characteristic = charz;

                Int32 Res = Client_n[k1].WriteClientConfiguration(Characteristic, true, wclGattOperationFlag.goNone);

                if (Res != wclErrors.WCL_E_SUCCESS && Res.ToString("X8").Substring(3, 5) != "51019")
                    MessageBox.Show("Error: 0x" + Res.ToString("X8") + "for" + k1 + "th device!(btWSub)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (Res.ToString("X8").Substring(3, 5) == "51009" || Res.ToString("X8").Substring(3, 5) == "50015")//Attribute not found/unexpected error
                {
                    MessageBox.Show(k1 + "th" + "Turned off or out of range");
                    not_found[k1] = 1;
                    sadd1[k1] = "-1";
                }
            }
        }

        private void Unsub_Click(object sender, EventArgs e)
        {
            wclGattCharacteristic charz;

            if (dict2.TryGetValue("8d164d06-a0f9-4755-9fff-a9d1a3ba3d19", out charz))
            {
                wclGattCharacteristic Characteristic = charz;

                Int32 Res = Client_n[k2].Unsubscribe(Characteristic);
                //Int32 Res1 = Client2.Unsubscribe(Characteristic);
                if (Res != wclErrors.WCL_E_SUCCESS && Res.ToString("X8").Substring(3, 5) != "5101A")//not subscribed
                    MessageBox.Show("Error: 0x" + Res.ToString("X8") + "for" + k2 + "th device!(btUnsub)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (Res.ToString("X8").Substring(3, 5) == "51009")//attribute not found
                {
                    MessageBox.Show(k2 + "th" + "Turned off or out of range");
                }
            }
        }

        private void UnsubW_Click(object sender, EventArgs e)
        {
            wclGattCharacteristic charz;

            //wclGattCharacteristic Characteristic = FCharacteristics[lvCharacteristics.SelectedItems[0].Index];
            if (dict2.TryGetValue("8d164d06-a0f9-4755-9fff-a9d1a3ba3d19", out charz))
            {
                wclGattCharacteristic Characteristic = charz;

                Int32 Res = Client_n[k2].WriteClientConfiguration(Characteristic, false, wclGattOperationFlag.goNone);
                if (Res != wclErrors.WCL_E_SUCCESS && Res.ToString("X8").Substring(3, 5) != "5101A")//not subscribed
                    MessageBox.Show("Error: 0x" + Res.ToString("X8") + "for" + k2 + "th device!(btWun)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (Res.ToString("X8").Substring(3, 5) == "51009")//attribute not found
                {
                    MessageBox.Show(k2 + "th" + "Turned off or out of range");
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (Client_n[d_index].State != wclCommunication.wclClientState.csConnected)
            {

            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            //Svice.PerformClick();
            //Chrstic.PerformClick();
            connected = 1;
        }

        private void Class1_Load(object sender, EventArgs e)
        {


        }

        public void sn_Click()
        {
            String sn_str;
            int mat = 0;
            wclGattCharacteristic charz;


            for (k1 = 0; k1 < d_count; k1++)
            {
                if (dict2.TryGetValue("5fe2c932-72aa-401e-a628-afc30b7a7612", out charz))
                {
                    wclGattCharacteristic Characteristic = charz;
                    //tadd[k1] = "0" + ((k1+1)).ToString();
                    tadd[k1] = "0" + ((k1 + 1));
                    sn_str = "$H" + tadd[k1] + "00+0";
                    sn_str = checksum(sn_str);
                    byte[] bytes = Encoding.ASCII.GetBytes(sn_str);//convert char array to byte array for transmission
                    for (int j = 0; j < 3; j++)
                    {

                        Int32 Res = Client_n[k1].WriteCharacteristicValue(Characteristic, bytes);


                        if (Res != wclErrors.WCL_E_SUCCESS)
                        {

                            //continue;
                            //MessageBox.Show("Error: 0x" + Res.ToString("X8"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            fault = 1;
                            temp = k1;
                            found[temp] = 0;
                            turn_off = 1;//one or more scales are off
                            dict.Remove(temp);//remove previous radio with which scale was connected
                                              //dict0.Remove(k1);
                                              /*if (Instance == 1)
                                              {
                                                  btDiscover_Click();
                                                  Instance = 0;
                                              }*/
                                              //timers[temp].Enabled = true;
                            discn[mat++] = temp;//values of temp stored here(the scale indexes)
                            break;
                        }
                        else
                        {
                            serial_Click = true;
                            //fault = 0;
                            break;
                        }

                    }
                }


                //btSubscribe_Click();
                //btWriteSubsc_Click();
            }


            //this section to start discovering lost scales again//-------------------
            if (/*Instance == 1*/fault == 1)
            {
                btDiscover_Click();
                //fault = 0;
                //Instance = 0;
            }
            for (int prev = 0; prev < 18; prev++)
            {
                if (discn[prev] != -1)
                {
                    timers[discn[prev]].Enabled = true;
                }
                //discn[prev] = -1;//restore default values of discn[]
            }
            //this section to start discovering lost scales again//-------------------

            k1 = 0;
            while (k1 < d_count)
            {
                //await Task.Delay(500);
                if (not_found[k1] == 0)
                {
                    btSubscribe_Click();
                    btWriteSubsc_Click();

                    //Sub.PerformClick();
                    //Wsub.PerformClick();
                    //await Task.Delay(500);
                }
                k1++;
            }
            notify = 0;
            if (scale_count == d_count)
            {
                scale_check = true;//to fill final weight text boxes from acquire ad RTZ
            }
            k1 = 0;
            fault = 0;


            //k1 = 0;
        }


        public void Scales_off()
        {
            wclGattCharacteristic charz;
            int x = 0;

            for (x = 0; x < d_count; x++)//acquire the chrstic,subscribe it, write CCCD
            {

                if (dict2.TryGetValue("5fe2c932-72aa-401e-a628-afc30b7a7612", out charz))
                {
                    wclGattCharacteristic Characteristic = charz;
                    /*if (lvCharacteristics.SelectedItems.Count == 0)//LV is List View
                    {
                        MessageBox.Show("Select characteristic", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    wclGattCharacteristic Characteristic = FCharacteristics[lvCharacteristics.SelectedItems[0].Index];*/

                    String Str = "$O3100";
                    Str = checksum(Str);
                    byte[] bytes = Encoding.ASCII.GetBytes(Str);
                    //for (int j = 0; j < 3; j++)
                    //{
                    Int32 Res = Client_n[x].WriteCharacteristicValue(Characteristic, bytes);
                    //if (Res != wclErrors.WCL_E_SUCCESS)
                    //  continue;
                    //else
                    //  break;

                    //}
                }
                //Int32 Res1 = Client2.WriteCharacteristicValue(Characteristic, Val);
                /*if (Res != wclErrors.WCL_E_SUCCESS)
                    MessageBox.Show("Error: 0x" + Res.ToString("X8"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (Res1 != wclErrors.WCL_E_SUCCESS)
                    MessageBox.Show("Error: 0x" + Res.ToString("X8"), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);*/
                //click on subscribe button and Write CCCD subscribe button only ONCE!
                //Basically we are reading the 2nd charaterstic in the following two functions

                /********WE DONT WANT ZERO COMMAND TO TURN ON INDICATION************/
                //btSubscribe_Click(btSubscribe, EventArgs.Empty);
                //btWriteSubsc_Click(btWriteSubsc, EventArgs.Empty);//after enabling indications-->charstc_changed
            }

        }



    }

    /*partial class Class1
    {


    }*/



}
