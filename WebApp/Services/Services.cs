using System;
using ToolsApp.com.netsuite.webservices;
using ToolsApp.Helper;

namespace ToolsApp.Services
{
    public class NetSuiteItemFulfillmentService 
    {      
        public static bool CreateSalesOrderFulfillment(string so_internalId, string soctxn, DateTime ngayct)
        {
            NSClient.NSClient _nsClient = new NSClient.NSClient();
            InitializeRecord ir = new InitializeRecord();
            ir.type = InitializeType.itemFulfillment;
            InitializeRef iref = new InitializeRef();
            iref.typeSpecified = true;
            iref.type = InitializeRefType.salesOrder;
            iref.internalId = so_internalId;
            ir.reference = iref;
            ReadResponse getInitResponse = _nsClient.Service.initialize(ir);
            if (getInitResponse.status.isSuccess)
            {
                ItemFulfillment ifrec = (ItemFulfillment)getInitResponse.record;
                ItemFulfillment recToFulfill = new ItemFulfillment();
                recToFulfill.createdFrom = ifrec.createdFrom;
                recToFulfill.tranId = soctxn + "_Fulfillment";
                recToFulfill.tranDate = ngayct;
                recToFulfill.tranDateSpecified = true;
                recToFulfill.shipStatus = ItemFulfillmentShipStatus._shipped;
                recToFulfill.shipStatusSpecified = true;
                ItemFulfillmentItemList ifitemList = ifrec.itemList;
                ItemFulfillmentItem[] ifitems = new ItemFulfillmentItem[ifitemList.item.Length];

                RecordRef locRef = new RecordRef();
                for (int i = 0; i < ifitemList.item.Length; i++)
                {
                    ItemFulfillmentItem ffItem = new ItemFulfillmentItem();
                    ffItem.item = ifitemList.item[i].item;
                    ffItem.orderLineSpecified = true;
                    ffItem.orderLine = ifitemList.item[i].orderLine;
                    ffItem.location = ifitemList.item[i].location;
                    ffItem.department = ifitemList.item[i].department;
                    ffItem.serialNumbers = ifitemList.item[i].serialNumbers;
                    ffItem.quantity = ifitemList.item[i].quantityRemaining;
                    ffItem.quantitySpecified = true;
               
                    ifitems[i] = ffItem;
                }
                ItemFulfillmentItemList ifitemlistToFulfill = new ItemFulfillmentItemList();
                ifitemlistToFulfill.item = ifitems;
                recToFulfill.itemList = ifitemlistToFulfill;
                if (_nsClient.UseTba)
                {
                    _nsClient.SetPreferences();
                }
                WriteResponse result = _nsClient.Service.add(recToFulfill);
                if (result.status.isSuccess == true)
                {
                    try
                    {
                        //_netsuiteTTRepository.LoggingSyncHDDT(showroom, sophieu, 2, "create item fulfillment");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception: " + ex.Message);
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                        }
                        throw;
                    }
                }
                else
                {
                   // _nsClient.AddHddtFailToCreateItemFulfillment(showroom, sophieu, result.status.statusDetail[0].message);
                }
                return result.status.isSuccess;
            }
            else
            {
                _nsClient.Out.Error(_nsClient.GetStatusDetails(getInitResponse.status));
                return false;
            }
        }
    }
}