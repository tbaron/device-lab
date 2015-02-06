$('#apkForm').submit(function (e) {
    var status = $('#uploadStatus');
    var submitButton = $('#submit');

    var formObj = $(this);
    var formURL = formObj.attr("action");
    var formData = new FormData(this);

    submitButton.prop('disabled', true);
    status.html("Uploading...");

    $.ajax({
        url: formURL,
        type: 'POST',
        data: formData,
        mimeType: "multipart/form-data",
        contentType: false,
        cache: false,
        processData: false,
        success: function (data, textStatus, jqXHR) {
            submitButton.prop('disabled', false);
            status.html("Uploaded: " + data);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            submitButton.prop('disabled', false);
            status.html("Error: " + errorThrown);
        }
    });

    e.preventDefault();
});

$(".input-clear-container").on("click", ".input-clear", function () {
    $(this).prev("input").val("");
});


$(document).on('change', '.btn-file :file', function () {
    var input = $(this),
        numFiles = input.get(0).files ? input.get(0).files.length : 1,
        label = input.val().replace(/\\/g, '/').replace(/.*\//, '');
    input.trigger('fileselect', [numFiles, label]);
});

$(document).ready(function () {
    $('.btn-file :file').on('fileselect', function (event, numFiles, label) {

        var input = $(this).parents('.input-group').find(':text'),
            log = numFiles > 1 ? numFiles + ' files selected' : label;

        if (input.length) {
            input.val(log);
        } else {
            if (log) alert(log);
        }

    });
});