using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using System.IO;
using CharityMS.ViewModels;

namespace CharityMS.Controllers
{
    public class InventoryController : Controller
    {
        private const string tableName = "inventorytp057978";

        private List<string> getCredentialInfo()
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");
            IConfigurationRoot configure = builder.Build(); 

            List<string> KeyList = new List<string>();
            KeyList.Add(configure["AWSCredential:key1"]); //access key
            KeyList.Add(configure["AWSCredential:key2"]); //secret key
            KeyList.Add(configure["AWSCredential:key3"]); //session token

            return KeyList;
        }

        public IActionResult Index(string msg = "")
        {
            ViewBag.msg = msg;
            return View();
        }

        public async Task<IActionResult> createTable()
        {
            List<string> KeyList = getCredentialInfo();
            var DynamoDbclientobject = new AmazonDynamoDBClient(KeyList[0], KeyList[1], KeyList[2], RegionEndpoint.USEast1);

            string message = "";
            try
            {
                var createTableRequest = new CreateTableRequest
                {
                    TableName = tableName,

                    AttributeDefinitions = new List<AttributeDefinition>()
                    {
                        new AttributeDefinition
                        {
                            AttributeName = "DonorID", 
                            AttributeType = "S"
                        },
                        new AttributeDefinition
                        {
                            AttributeName = "InventoryID", 
                            AttributeType = "S"
                        }
                    },

                    KeySchema = new List<KeySchemaElement>()
                    {
                        new KeySchemaElement 
                        {
                            AttributeName = "DonorID",
                            KeyType = "HASH"
                        },
                        new KeySchemaElement 
                        {
                            AttributeName = "InventoryID",
                            KeyType = "RANGE"
                        }
                    },

                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 2,
                        WriteCapacityUnits = 1,
                    },
                };

                await DynamoDbclientobject.CreateTableAsync(createTableRequest);
                message = tableName + " is successfully created in the DynamoDB now!";
            }
            catch (AmazonDynamoDBException ex)
            {
                message = "Error in creating " + tableName + " : " + ex.Message;
            }
            catch (Exception ex)
            {
                message = "Error in creating " + tableName + " : " + ex.Message;
            }

            return RedirectToAction("Index", "Inventory", new { msg = message });
        }

        public IActionResult InsertInventory(string msg = "") 
        {
            ViewBag.msg = msg;
            return View(new InventoryVM());
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsertInventory(InventoryVM vm)
        {
            
            List<string> KeyList = getCredentialInfo();
            var DynamoDbclientobject = new AmazonDynamoDBClient(KeyList[0], KeyList[1], KeyList[2], RegionEndpoint.USEast1);

            
            Dictionary<string, AttributeValue> documentRecord = new Dictionary<string, AttributeValue>();
            string message = "";

            
            try
            {
                
                documentRecord["DonorID"] = new AttributeValue { S = vm.DonorID };
                documentRecord["InventoryID"] = new AttributeValue { S = Guid.NewGuid().ToString() };
                documentRecord["Product_Name"] = new AttributeValue { S = vm.Name };
                documentRecord["Quantity"] = new AttributeValue { N = vm.Quantity.ToString() };
                documentRecord["Inventory_Category"] = new AttributeValue { S = vm.Category };
                documentRecord["Inventory_Status"] = new AttributeValue { S = "Available" };

                //dynamic structure for the attribute section
                if (vm.Category == "food" || vm.Category == "beverage")
                {
                    if (!string.IsNullOrEmpty(vm.ExpiredDate.ToString()))
                    {
                        documentRecord["Expierd_Date"] = new AttributeValue { S = vm.ExpiredDate.ToString() };
                    }
                }
                else if(vm.Category=="electronics")
                {
                    if (!string.IsNullOrEmpty(vm.ProductCondition))
                    {
                        documentRecord["Product_Condition"] = new AttributeValue { S = vm.ProductCondition };
                    }
                }
                else if (vm.Category == "clothe")
                {
                    if (!string.IsNullOrEmpty(vm.cloteSize))
                    {
                        documentRecord["Clothe_Size"] = new AttributeValue { S = vm.cloteSize };
                    }
                }

                PutItemRequest request = new PutItemRequest
                {
                    TableName = tableName,
                    Item = documentRecord
                };

                await DynamoDbclientobject.PutItemAsync(request);
                message = "Donation from of user - " + vm.DonorID + " is added to the Inventory table now! ";
            }
            catch (AmazonDynamoDBException ex)
            {
                message = "Error in inserting inventory for donor of " + vm.DonorID + " : " + ex.Message;
            }
            catch (Exception ex)
            {
                message = "Error in inserting inventory for donor of " + vm.DonorID + " : " + ex.Message;
            }

            return RedirectToAction("InsertInventory", "Inventory", new { msg = message });
        }

        
        public async Task<IActionResult> InventoryListAsync(string msg = "") 
        {
            
            List<string> KeyList = getCredentialInfo();
            var DynamoDbclientobject = new AmazonDynamoDBClient(KeyList[0], KeyList[1], KeyList[2], RegionEndpoint.USEast1);

            
            List<Document> returnRecords = new List<Document>(); 
            List<KeyValuePair<string, string>> singleConvertedRecord = new List<KeyValuePair<string, string>>(); 
            List<List<KeyValuePair<string, string>>> fullList = new List<List<KeyValuePair<string, string>>>();
            string message = "";

            
            try
            {
                
                ScanFilter scanprice = new ScanFilter();

                
                Table customerTransactions = Table.LoadTable(DynamoDbclientobject, tableName);
                Search search = customerTransactions.Scan(scanprice); 


                do
                {
                    returnRecords = await search.GetNextSetAsync(); 

                    if (returnRecords.Count == 0)
                    {
                        ViewBag.msg = "Record is not found! ";
                        return View(fullList);
                    }

                   
                    foreach (var singlerecord in returnRecords)
                    {
                        foreach (var attributeKey in singlerecord.GetAttributeNames()) 
                        {
                            string attributeValue = "";

                            if (singlerecord[attributeKey] is DynamoDBBool) 
                                attributeValue = singlerecord[attributeKey].AsBoolean().ToString();
                            else if (singlerecord[attributeKey] is Primitive) 
                                attributeValue = singlerecord[attributeKey].AsPrimitive().Value.ToString();
                            else if (singlerecord[attributeKey] is PrimitiveList)
                                attributeValue = string.Join(",", (from primitive
                                                in singlerecord[attributeKey].AsPrimitiveList().Entries
                                                                   select primitive.Value).ToArray());

                            singleConvertedRecord.Add(new KeyValuePair<string, string>(attributeKey, attributeValue));
                        }

                        fullList.Add(singleConvertedRecord);
                        singleConvertedRecord = new List<KeyValuePair<string, string>>();
                    }
                }
                while (!search.IsDone);

                ViewBag.msg = "Data was found!";
                List<InventoryVM> vmList = new List<InventoryVM>();
                foreach (var item in fullList)
                {
                    InventoryVM vm = new InventoryVM();
                    foreach (var i in item)
                    {
                        switch (i.Key)
                        {
                            case "DonorID":
                                vm.DonorID = i.Value;
                                break;
                            case "InventoryID":
                                vm.InventoryID = i.Value;
                                break;
                            case "Expierd_Date":
                                vm.ExpiredDate = i.Value;
                                break;
                            case "Inventory_Category":
                                vm.Category = i.Value;
                                break;
                            case "Product_Condition":
                                vm.ProductCondition = i.Value;
                                break;
                            case "Product_Name":
                                vm.Name = i.Value;
                                break;
                            case "Quantity":
                                vm.Quantity = i.Value;
                                break;
                            case "Inventory_Status":
                                vm.Status = i.Value;
                                break;
                            case "Clothe_Size":
                                vm.Status = i.Value;
                                break;
                        }
                    }
                    vmList.Add(vm);
                }
                return View(vmList);
            }
            catch (AmazonDynamoDBException ex)
            {
                message = "Error in retrieve the data : " + ex.Message;
            }
            catch (Exception ex)
            {
                message = "Error in retrieve the data : " + ex.Message;
            }

            ViewBag.msg= message;
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InventoryList(string category, string status, string pruductName)
        {
            
            List<string> KeyList = getCredentialInfo();
            var DynamoDbclientobject = new AmazonDynamoDBClient(KeyList[0], KeyList[1], KeyList[2], RegionEndpoint.USEast1);

            
            List<Document> returnRecords = new List<Document>(); 
            List<KeyValuePair<string, string>> singleConvertedRecord = new List<KeyValuePair<string, string>>(); 
            List<List<KeyValuePair<string, string>>> fullList = new List<List<KeyValuePair<string, string>>>();
            string message = "";

            try
            {
                ScanFilter scanprice = new ScanFilter();
                if (!String.IsNullOrEmpty(category))
                    scanprice.AddCondition("Inventory_Category", ScanOperator.Contains, category);
                if (!String.IsNullOrEmpty(pruductName))
                    scanprice.AddCondition("Product_Name", ScanOperator.Contains, pruductName);
                if (!String.IsNullOrEmpty(status))
                    scanprice.AddCondition("Inventory_Status", ScanOperator.Contains, status);

                Table customerTransactions = Table.LoadTable(DynamoDbclientobject, tableName);
                Search search = customerTransactions.Scan(scanprice);
                    

                do
                {
                    returnRecords = await search.GetNextSetAsync();

                    if (returnRecords.Count == 0)
                    {
                        ViewBag.msg = "Record is not found! ";
                        return View(new List<InventoryVM>());
                    }

                    foreach (var singlerecord in returnRecords)
                    {
                        foreach (var attributeKey in singlerecord.GetAttributeNames())
                        {
                            string attributeValue = "";

                            if (singlerecord[attributeKey] is DynamoDBBool)
                                attributeValue = singlerecord[attributeKey].AsBoolean().ToString();
                            else if (singlerecord[attributeKey] is Primitive) 
                                attributeValue = singlerecord[attributeKey].AsPrimitive().Value.ToString();
                            else if (singlerecord[attributeKey] is PrimitiveList)
                                attributeValue = string.Join(",", (from primitive
                                                in singlerecord[attributeKey].AsPrimitiveList().Entries
                                                                   select primitive.Value).ToArray());

                            singleConvertedRecord.Add(new KeyValuePair<string, string>(attributeKey, attributeValue));
                        }

                        fullList.Add(singleConvertedRecord);
                        singleConvertedRecord = new List<KeyValuePair<string, string>>();
                    }
                }
                while (!search.IsDone);

                ViewBag.msg = "Data was found!";

                List<InventoryVM> vmList = new List<InventoryVM>();
                foreach(var item in fullList)
                {
                    InventoryVM vm = new InventoryVM();
                    foreach (var i in item)
                    {
                        switch (i.Key)
                        {
                            case "DonorID":
                                vm.DonorID = i.Value;
                                break;
                            case "InventoryID":
                                vm.InventoryID = i.Value;
                                break;
                            case "Expierd_Date":
                                vm.ExpiredDate = i.Value;
                                break;
                            case "Inventory_Category":
                                vm.Category = i.Value;
                                break;
                            case "Product_Condition":
                                vm.ProductCondition = i.Value;
                                break;
                            case "Product_Name":
                                vm.Name = i.Value;
                                break;
                            case "Quantity":
                                vm.Quantity = i.Value;
                                break;
                            case "Inventory_Status":
                                vm.Status = i.Value;
                                break;
                            case "Clothe_Size":
                                vm.Status = i.Value;
                                break;
                        }
                    }
                    vmList.Add(vm);
                }
                return View(vmList);
            }
            catch (AmazonDynamoDBException ex)
            {
                message = "Error in retrieve the data : " + ex.Message;
            }
            catch (Exception ex)
            {
                message = "Error in retrieve the data : " + ex.Message;
            }

            return RedirectToAction("InventoryList", "Inventory", new { msg = message });

        }

        [HttpGet]
        public async Task<IActionResult> updateInventory(string donorId, string inventoryId)
        {
            List<string> keyLists = getCredentialInfo();

            var dynamoDBClientObject = new AmazonDynamoDBClient(keyLists[0], keyLists[1], keyLists[2], RegionEndpoint.USEast1);
            string message = "";

            List<Document> singleReturnRecordItem = new List<Document>();
            List<KeyValuePair<string, string>> changeValuedSingleItem = new List<KeyValuePair<string, string>>();

            try
            {
                QueryFilter queryFilter = new QueryFilter("DonorID", QueryOperator.Equal, donorId);
                queryFilter.AddCondition("InventoryID", QueryOperator.Equal, inventoryId);

                Table searchSingleProduct = Table.LoadTable(dynamoDBClientObject, tableName);
                Search search = searchSingleProduct.Query(queryFilter);

                singleReturnRecordItem = await search.GetNextSetAsync();
                if (singleReturnRecordItem.Count <= 0)
                {
                    return RedirectToAction("InventoryList", "Inventory", new { msg = "No Record Was Found" });
                }

                foreach (var singlerecord in singleReturnRecordItem)
                {
                    foreach (var attributeKey in singlerecord.GetAttributeNames())
                    {
                        string attributeValue = "";

                        if (singlerecord[attributeKey] is DynamoDBBool)
                            attributeValue = singlerecord[attributeKey].AsBoolean().ToString();
                        else if (singlerecord[attributeKey] is Primitive)
                            attributeValue = singlerecord[attributeKey].AsPrimitive().Value.ToString();
                        else if (singlerecord[attributeKey] is PrimitiveList)
                            attributeValue = string.Join(",", (from primitive
                                            in singlerecord[attributeKey].AsPrimitiveList().Entries
                                                               select primitive.Value).ToArray());

                        changeValuedSingleItem.Add(new KeyValuePair<string, string>(attributeKey, attributeValue));
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return RedirectToAction("InventoryList", "Inventory", new { msg = message });
            }

            InventoryVM vm = new InventoryVM();

            foreach(var i in changeValuedSingleItem){
                switch (i.Key)
                {
                    case "DonorID":
                        vm.DonorID = i.Value;
                        break;
                    case "InventoryID":
                         vm.InventoryID = i.Value;
                        break;
                    case "Expierd_Date":
                        vm.ExpiredDate = i.Value;
                        break;
                    case "Inventory_Category":
                        vm.Category = i.Value;
                        break;
                    case "Product_Condition":
                        vm.ProductCondition = i.Value;
                        break;
                    case "Product_Name":
                        vm.Name = i.Value;
                        break;
                    case "Quantity":
                        vm.Quantity = i.Value;
                        break;
                    case "Inventory_Status":
                        vm.Status = i.Value;
                        break;
                    case "Clothe_Size":
                        vm.Status = i.Value;
                        break;
                }
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> updateData(InventoryVM vm)
        {
            List<string> KeyList = getCredentialInfo();
            
            var DynamoDbclientobject = new AmazonDynamoDBClient(KeyList[0], KeyList[1], KeyList[2], RegionEndpoint.USEast1);
            
            List<KeyValuePair<string, AttributeValue>> updateListItem = new List<KeyValuePair<string, AttributeValue>>();
            updateListItem.Add(new KeyValuePair<string, AttributeValue>("Quantity", new AttributeValue { N = vm.Quantity }));
            updateListItem.Add(new KeyValuePair<string, AttributeValue>("Inventory_Status", new AttributeValue { S = vm.Status }));
            updateListItem.Add(new KeyValuePair<string, AttributeValue>("Product_Name", new AttributeValue { S = vm.Name }));
            if (vm.ExpiredDate!=null)
            {
                updateListItem.Add(new KeyValuePair<string, AttributeValue>("Expierd_Date", new AttributeValue { S=vm.ExpiredDate.ToString() }));
            }
            if (!string.IsNullOrEmpty(vm.ProductCondition))
            {
                updateListItem.Add(new KeyValuePair<string, AttributeValue>("Product_Condition", new AttributeValue { S = vm.ProductCondition }));
            }
            if (!string.IsNullOrEmpty(vm.cloteSize))
            {
                updateListItem.Add(new KeyValuePair<string, AttributeValue>("Clothe_Size", new AttributeValue { S = vm.cloteSize }));
            }

            foreach (var singleAttribute in updateListItem)
            {
                var value = new Dictionary<string, AttributeValue>();
                value.Add(":value", singleAttribute.Value);
                var request = new UpdateItemRequest
                {
                    TableName = tableName,
                    Key = new Dictionary<string, AttributeValue>()
                    {
                        {"DonorID", new AttributeValue{S=vm.DonorID} },
                        {"InventoryID", new AttributeValue{S=vm.InventoryID} }
                    },
                    ExpressionAttributeValues = value,
                    UpdateExpression = "SET " + singleAttribute.Key + " = :value"
                };
                await DynamoDbclientobject.UpdateItemAsync(request);
            }
            return RedirectToAction("InventoryList", "Inventory", new
            {
                msg = "Record of customer " + vm.DonorID + " is updated now!"
            });
        }

        
        public async Task<IActionResult> deleteInventory(string donorId, string inventoryId)
        {
            List<string> KeyList = getCredentialInfo();

            var DynamoDbclientobject = new AmazonDynamoDBClient(KeyList[0], KeyList[1], KeyList[2], RegionEndpoint.USEast1);

            var request = new DeleteItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>() {
                    { "DonorID", new AttributeValue { S = donorId } },
                    { "InventoryID", new AttributeValue { S = inventoryId } }
                }
            };

            await DynamoDbclientobject.DeleteItemAsync(request);

            return RedirectToAction("InventoryList", "Inventory", new
            {
                msg = "The item is deleted"
            });
        }
    }
}
