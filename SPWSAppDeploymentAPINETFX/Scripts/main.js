//import { renderTemplateForEach } from "knockout";

console.log("main function");
var viewModel;


function Client(ConnectionId, HostName, IPAddress) {
    var self = this;
    self.ConnectionId = ko.observable(ConnectionId);
    self.HostName = ko.observable(HostName);
    self.IPAddress = ko.observable(IPAddress);
    self.Url = (getClientTaskUrl + "/" + self.HostName());
    self.GetTasks = function () {
        $.ajax(self.Url,
            {
                method: "GET",
                dataType: 'application/json',
                statusCode:
                {
                    200: function (data) {
                        console.log(data);
                        viewModel.Tasks([]);
                        //var processedText = data.responseText.replaceAll("\\", "");
                        //processedText = processedText.replaceAll("\"", '"');
                        //processedText = processedText.trim('\'');
                        var parsedData = JSON.parse(data.responseText);
                        parsedData = JSON.parse(parsedData);
                        for (var i = 0; i < parsedData.length; i++) {
                            var currentData = parsedData[i];
                            var task = new Task(currentData.ImageName, currentData.PID, currentData.MemUsage);
                            viewModel.Tasks.push(task);
                        }
                        viewModel.SelectedClient(self);
                    }
                }
            }
        )
    }
}

function Url(Name, Url) {
    var self = this;
    self.Name = ko.observable(Name);
    self.Url = ko.observable(Url);
}

function Task(ImageName,PID,MemUsage) {
    var self = this;
    self.ImageName = ko.observable(ImageName);
    self.PID = ko.observable(PID);
    self.MemUsage = ko.observable(MemUsage);
    self.KillTask = function () {
        var url = killClientTaskUrl + "/" + viewModel.SelectedClient().HostName() + "/" + self.PID();
        $.ajax(url,
            {
                method: "GET",
                dataType: 'application/json',
                statusCode:
                {
                    200: function (data) {
                        console.log(data);
                    }
                }
            }
        );
    }
}

function RefreshClients() {

    $.ajax(getClientsUrl,
        {
            method: "GET",
            dataType: "application/json",
            statusCode:
            {
                200: function (data) {
                    console.log(data);
                    //var processedText = data.responseText.replaceAll("\\", "");
                    //processedText = processedText.replaceAll("\"", '"');
                    var processedText = data.responseText.replaceAll('""','"');
                    var parsedData = JSON.parse(processedText);
                    parsedData = JSON.parse(parsedData);
                    viewModel.Clients([]);
                    for (var i = 0; i < parsedData.length; i++) {
                        var currentData = parsedData[i];

                        var client = new Client(currentData.ConnectionId, currentData.HostName, currentData.IPAddress);

                        viewModel.Clients.push(client);

                    }

                }
            }
        });
}

function ViewModel() {
    var self = this;
    self.Tasks = ko.observableArray();
    self.Clients = ko.observableArray();
    self.Urls = ko.observableArray();
    self.SelectedClient = ko.observable();
}