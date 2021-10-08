using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event
{
    public class CustomerEvent
    {
        /// <summary>
        /// 无参的自定义事件
        /// </summary>

            //1.声明关于事件的委托；
            public delegate void CustomerEventHander(object sender, EventArgs e);
            //2.声明事件；   
            public event CustomerEventHander customerEventHander;
            //3.编写引发事件的函数；
            public void customerEvent()
            {
                if (this.customerEventHander != null)
                {
                    this.customerEventHander(this, new EventArgs());
                }
            }
        }
    
}

