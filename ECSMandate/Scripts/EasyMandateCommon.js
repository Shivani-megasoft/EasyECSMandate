
/*Automatically bind json data to form. */
var isDebug = false;
function bindFormData(formId, jData) {
    debug("");
    var $form = $(formId);

    $.each(jData, function (fieldName, val) {
        debug("Field : " + fieldName + "=>" + val);

        var $formField = $("#" + fieldName, $form);

        if ($formField.is(":checkbox")) {
            $formField.attr("checked", val);
            $formField.attr("value", val);
        }
        if ($formField.is(":radio") && $formField.attr("value") === val) {
            $formField.attr("checked", val);
        }
        if ($formField.is(":text") || $formField.is("select") || $formField.is(":hidden") || $formField.is("textarea")) {
            if ($formField.attr("data-type") === "date" || $formField.hasClass("date")) {
                if (val !== null) {
                    //alert(val);
                    $formField.val(val.parseDate() || "");
                }
                else {
                    $formField.val("");
                }
            }
            else {
                if (val === 'undefined' || val === null) {
                    $formField.val("");
                }
                else {
                    $formField.val(val);
                    $formField.attr("title", val);
                }
            }
        }
        if ($formField.is("label")) {
            $formField.text(val);
        }
    });
}


function debug(val, clear) {
    if (isDebug) {
        if (clear) { $("#debug").html(""); }
        $("#debug").append(val);
    }
}


String.prototype.parseDate = function () {
    //console.log(this);
    var m = this.match(/^\/Date\((\d+)\)\/$/);
    var date = null;
    //alert(m);
    //m = 568704600000;
    //alert(new Date(568704600000));
    if (m) {
        date = new Date(parseInt(m[1]));
        //console.log("m:", date);
    }
    return formatJSONDate(date);
}

function formatJSONDate(jsonDate) {
    if (null != jsonDate) {
        //var newDate = dateFormat(jsonDate.toLocaleDateString(), "dd/mm/yyyy");
        //pritam change
        var newDate = pad(jsonDate.getDate(), 2, '0') + "/" + pad((jsonDate.getMonth() + 1), 2, '0') + "/" + jsonDate.getFullYear();

        //        if (parseFloat($.browser.version) > 45) {
        //            var newDate = pad((jsonDate.getMonth() + 1), 2, '0') + "/" + pad(jsonDate.getDate(), 2, '0') + "/" + jsonDate.getFullYear();
        //        }
        //        else {
        //            var newDate = pad(jsonDate.getDate(), 2, '0') + "/" + pad((jsonDate.getMonth() + 1), 2, '0') + "/" + jsonDate.getFullYear();
        //        }
        //END
        return newDate;
    }
    else return "";
}

function formatDate(date) {
    if (null != date) {
        //var newDate = dateFormat(date.toLocaleDateString(), "dd/mm/yyyy");
        var newDate = pad(date.getDate(), 2, '0') + "/" + pad((date.getMonth() + 1), 2, '0') + "/" + date.getFullYear();
        return newDate;
    }
    else return "";
}



function pad(number, length, char) {

    var str = '' + number;
    while (str.length < length) {
        str = char + str;
    }

    return str;
}