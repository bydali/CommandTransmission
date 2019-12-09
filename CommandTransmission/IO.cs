using DSIM.Communications;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandTransmission
{
    class IO
    {
        private static IEventAggregator eventAggregator;
        private static MQHelper _mqHelper;

        public static void ReceiveMsg(IEventAggregator aggregator)
        {
            //eventAggregator = aggregator;

            //MQHelper.ConnectionString = ConfigurationManager.ConnectionStrings["RabbitMQ"].ConnectionString;
            //_mqHelper = new MQHelper
            //{
            //    ClientSubscriptionId = ConfigurationManager.ConnectionStrings["ClientID"].ConnectionString
            //};
            //_mqHelper.MessageArrived += RabbitMQ_MessageArrived;


        }

        private static void RabbitMQ_MessageArrived(object sender, MsgCategoryEnum e)
        {
            eventAggregator.GetEvent<PubSubEvent<string>>().Publish("asdf");
        }

    }
}
