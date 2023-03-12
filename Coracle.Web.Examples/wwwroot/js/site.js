// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

var captureFunction = function (url, value) {
    fetch(url, {
        method: 'get',
    })
        .then(function (response) {
            response.text().then(function (str) {
                console.log(`Capture ${value} fetch call returned: ${str}`);
            }).catch(function (err) {
                console.log(`Capture ${value} fetch call response parsing had an error: ${err}`);
            });

            if (value == 'start') {

                $('#stopCaptureBtn').css('display', 'block');
                $('#startCaptureBtn').css('display', 'none');
            }
            else {
                $('#startCaptureBtn').css('display', 'block');
                $('#stopCaptureBtn').css('display', 'none');
            }
        })
        .catch(function (err) {
            console.log(`Capture ${value} fetch call had an error: ${err}`);

            if (value != 'start') {

                $('#stopCaptureBtn').css('display', 'block');
                $('#startCaptureBtn').css('display', 'none');
            }
            else {
                $('#startCaptureBtn').css('display', 'block');
                $('#stopCaptureBtn').css('display', 'none');
            }
        });
}

var startCapture = function (e) {

    let val = 'start';

    console.log('Starting Capture');

    let url = '/Home/Capture?' + new URLSearchParams({
        value: val,
    });

    captureFunction(url, val);
};

var stopCapture = function (e) {

    let val = 'stop';

    console.log('Stopping Capture');

    let url = '/Home/Capture?' + new URLSearchParams({
        value: val,
    });

    captureFunction(url, val);
};

var clearCapture = function (e) {

    console.log('Clearing Capture');

    removeChildren('logList');
};

