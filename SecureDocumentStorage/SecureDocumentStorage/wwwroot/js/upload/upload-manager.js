var uploadManager = function () {
    var droppedFiles;
    var fileName;
    var $dropzone;
    var $button;
    var uploading;
    var $syncing;
    var $done;
    var $bar;
    var timeOut;
    var $input;


    //Initialisation 
    var init = function () {
        droppedFiles = false;
        fileName = '';
        $dropzone = $('.dropzone');
        $button = $('.upload-btn');
        uploading = false;
        $syncing = $('.syncing');
        $done = $('.done');
        $bar = $('.bar');
        $input = $("#upload-document");

        loadEvents();
    }

    //Initialisation events
    var loadEvents = function () {

        $dropzone.on('drag dragstart dragend dragover dragenter dragleave drop', function (e) {
            e.preventDefault();
            e.stopPropagation();
        })
            .on('dragover dragenter', function () {
                $dropzone.addClass('is-dragover');
            })
            .on('dragleave dragend drop', function () {
                $dropzone.removeClass('is-dragover');
            })
            .on('drop', function (e) {
                droppedFiles = e.originalEvent.dataTransfer.files;
                $input[0].files = droppedFiles;
                fileName = droppedFiles[0]['name'];
                $('.filename').html(fileName);
                $('.dropzone .upload').hide();
            });

        $button.bind('click', function () {
            startUpload();
        });

        $("input:file").change(function () {
            fileName = $(this)[0].files[0].name;
            $('.filename').html(fileName);
            $('.dropzone .upload').hide();
        });

    }

    var startUpload = function () {
        if (!uploading && fileName != '') {
            uploading = true;
            $button.html('Uploading...');
            $dropzone.fadeOut();
            $syncing.addClass('active');
            $done.addClass('active');
            $bar.addClass('active');
            $("#upload-form").submit();
        }
    }

    var showDone = function () {
        $button.html('Done');
    }

    var submitAesKey = function () {
        if ($("#aesKey").val() != "") {
            $("#keyFrame").hide();
            $("#uploadDocument").show();
        }
    }

    return {
        init: init,
        submitAesKey: submitAesKey
    }
}();