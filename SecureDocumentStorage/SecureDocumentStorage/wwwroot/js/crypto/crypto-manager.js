var cryptoManager = function ()
{
    var init = function () {

    }

    var load = function () {
    }

    var encryptFile = function (event)
    {
        var a = $('#down');
        var file = event.target.files[0];

        var reader = new FileReader();

        reader.onload = function (e)
        {
            var encryptedContext = CryptoJS.AES.encrypt(e.target.result, $("#aesKey").val());
            $("#encryptedFileStream").val(encryptedContext.toString());
        };

        $("#fileName").val(file.name);
        reader.readAsDataURL(file);
    }

    var decryptFile = function (encryptedDocument, fileName, key)
    {
       var decrypted = CryptoJS.AES.decrypt(encryptedDocument, key).toString(CryptoJS.enc.Latin1);

       if (!/^data:/.test(decrypted))
       {
           alert("Invalid pass phrase or file! Please try again.");
           return false;
        }
        $(".downloadFile").attr('href', decrypted);
        $(".downloadFile").attr('download', fileName);
    }

    return {
        init: init,
        encryptFile: encryptFile,
        decryptFile: decryptFile
    }
}();
