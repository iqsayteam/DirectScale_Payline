using PaylineDirectScale.Payline.Model;
using ServiceReference1;
using System;
using System.Net;
using System.ServiceModel;

namespace PaylineDirectScale.Payline.Utils
{
    public static class PaymentApiFactory
    {
        //public static PaylineProperties PaylineSDKProperties
        //{
        //    get
        //    {
        //        if (_properties == null)
        //            _properties = LoadConfiguration();
        //        return _properties;
        //    }
        //}

        private static PaylineProperties _properties = null;

        public static void InitClient()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate
            {
                return true;
            };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }
        //private static PaylineProperties LoadConfiguration()
        //{
        //    // Debug.WriteLine("Payline SDK - trying to load configuration file from " + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "payline.properties.xml"));
        //    //Get properties from payline.properties.xml
        //    return ((PaylineProperties)SerialXML.Load(AppDomain.CurrentDomain.BaseDirectory, "payline.properties.xml", typeof(PaylineProperties)));
        //}

        //public static ChannelFactory<WebPaymentAPI> GetWebFactory()
        //{
        //    var factory = new ChannelFactory<WebPaymentAPI>(CreateBindingAndInitializeClient(), new EndpointAddress(new Uri(GetWebUrl())));
        //    if (PaylineSDKProperties.Production)
        //    {
        //        factory.Credentials.UserName.UserName = PaylineSDKProperties.merchantID;
        //        factory.Credentials.UserName.Password = PaylineSDKProperties.accessKey;
        //    }
        //    else
        //    {
        //        factory.Credentials.UserName.UserName = PaylineSDKProperties.homoMerchantID;
        //        factory.Credentials.UserName.Password = PaylineSDKProperties.homoAccessKey;
        //    }
        //    return factory;
        //}

        //public static ChannelFactory<DirectPaymentAPI> GetDirectFactory()
        //{
        //    var factory = new ChannelFactory<DirectPaymentAPI>(CreateBindingAndInitializeClient(), new EndpointAddress(new Uri(GetDirectUrl())));
        //    if (PaylineSDKProperties.Production)
        //    {
        //        factory.Credentials.UserName.UserName = PaylineSDKProperties.merchantID;
        //        factory.Credentials.UserName.Password = PaylineSDKProperties.accessKey;
        //    }
        //    else
        //    {
        //        factory.Credentials.UserName.UserName = PaylineSDKProperties.homoMerchantID;
        //        factory.Credentials.UserName.Password = PaylineSDKProperties.homoAccessKey;
        //    }
        //    return factory;
        //}


        //public static WebPaymentAPI GetServiceProxy(ChannelFactory<WebPaymentAPI> factory)
        //{
        //    var serviceProxy = factory.CreateChannel();
        //    ((ICommunicationObject)serviceProxy).Open();
        //    return serviceProxy;
        //}
        public static DirectPaymentAPI GetDirectServiceProxy(ChannelFactory<DirectPaymentAPI> factory)
        {
            var serviceProxy = factory.CreateChannel();
            ((ICommunicationObject)serviceProxy).Open();
            return serviceProxy;
        }
        //public static void DisposeServiceProxy(ChannelFactory<WebPaymentAPI> factory, WebPaymentAPI serviceProxy, OperationContext prevOpContext)
        //{
        //    factory.Close();
        //    ((ICommunicationObject)serviceProxy).Close();
        //    OperationContext.Current = prevOpContext;
        //}

        public static void DisposeServiceProxy(ChannelFactory<DirectPaymentAPI> factory, DirectPaymentAPI serviceProxy, OperationContext prevOpContext)
        {
            factory.Close();
            ((ICommunicationObject)serviceProxy).Close();
            OperationContext.Current = prevOpContext;
        }

        public static BasicHttpBinding CreateBindingAndInitializeClient()
        {
            var basicHttpBinding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
            basicHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            InitClient();
            return basicHttpBinding;
        }

        //private static string GetWebUrl()
        //{
        //    if (PaylineSDKProperties.Production)
        //        return PaylineSDKProperties.WebPaymentAPIUrlProd;
        //    else
        //        return PaylineSDKProperties.WebPaymentAPIUrl;
        //}
        //private static string GetDirectUrl()
        //{
        //    if (PaylineSDKProperties.Production)
        //        return PaylineSDKProperties.DirectPaymentAPIUrlProd;
        //    else
        //        return PaylineSDKProperties.DirectPaymentAPIUrl;
        //}

        //public static string ContractNumbers()
        //{
        //    if (PaylineSDKProperties.Production)
        //        return PaylineSDKProperties.ContractNumber;
        //    else
        //        return PaylineSDKProperties.homoContractNumber;
        //}

        //public static string CancelUrl()
        //{
        //    if (PaylineSDKProperties.Production)
        //        return PaylineSDKProperties.DefaultCancelUrl;
        //    else
        //        return PaylineSDKProperties.homoCancelUrl;
        //}
             
    }
}
