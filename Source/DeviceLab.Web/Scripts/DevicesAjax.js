var DEVICE_STATUS_INTERVAL = 1000;

function nano(template, data) {
    return template.replace(/\{([\w\.]*)\}/g, function (str, key) {
        var keys = key.split("."), v = data[keys.shift()];
        for (var i = 0, l = keys.length; i < l; i++) v = v[keys[i]];
        return (typeof v !== "undefined" && v !== null) ? v : "";
    });
}

var lastDeviceRefreshToken;

function updateDeviceStatus() {
    $.ajax({
        url: "/api/device/list",
        data: {
            refreshToken: lastDeviceRefreshToken,
            timeout: 30
        },
        dataType: "json",
        success: function (data) {
            if (data && data.refreshToken !== lastDeviceRefreshToken) {
                lastDeviceRefreshToken = data.refreshToken;

                var template = $("#deviceTemplate").html();

                var htmls = $.map(data.devices, function (item, i) {
                    return nano(template, item);
                });

                $("#deviceContainer").html(htmls.join(""));
            }
        },
        complete: function () {
            setTimeout(function () {
                updateDeviceStatus();
            }, DEVICE_STATUS_INTERVAL);
        }
    });
}

updateDeviceStatus();


function resetAll() {
    $(".device-control-pane").removeClass("active");
    $("#launchUrlTextbox").val("");
    $("#apk,#apk-text").val("");
}

function handleApkJobStatus(data) {
    resetAll();
    $("#panel-app-job").addClass("active");
    $("#apk-text").val(data.apkUrl.replace(/.+\//, ""));
}

function handleUrlJobStatus(data) {
    resetAll();
    $("#panel-url-job").addClass("active");
    $("#launchUrlTextbox").val(data.url);
}

function handleStressTestJobStatus(data) {
    resetAll();
    $("#panel-stresstest-job").addClass("active");
}

var jobTypeHandlers = {
    "RunAppServiceJob": handleApkJobStatus,
    "RunUrlServiceJob": handleUrlJobStatus,
    "RunStressTestServiceJob": handleStressTestJobStatus,
};

function parseJobType(data) {
    var rawType = data && data["$type"];
    var parsed = /(\w+),/.exec(rawType);

    return parsed && parsed[1];
}

var jobRefreshToken;
var lastJobId;

function updateJobStatus() {
    $.ajax({
        url: "/api/service/job",
        data: {
            refreshToken: jobRefreshToken,
            timeout: 30
        },
        dataType: "json",
        success: function (data) {
            if (data && data.refreshToken != jobRefreshToken) {
                jobRefreshToken = data.refreshToken;

                var job = data.job;
                var jobType = parseJobType(job);
                var handler = jobTypeHandlers[jobType];

                if (handler) {
                    handler(job);
                } else {
                    resetAll();
                }
            }
        },
        complete: function () {
            setTimeout(function () {
                updateJobStatus();
            }, DEVICE_STATUS_INTERVAL);
        }
    });
}

updateJobStatus();
