/*
* Copyright (c) 2011 Rajesh Pillai (pillai dot rajesh at  gmail dot com)
* Licensed under the MIT License:
* http://www.opensource.org/licenses/mit-license.php
*
*/

/*
* Navgate page elements by using enter key.  
*/

(function ($) {

    jQuery.fn.enter2tab = function () {
        var inputElements = $(this);
        var index = 0;
        $(inputElements).bind('keydown', function (event) {
            if (event.which === 13) { // if enter key pressed
             
                if ($(this).attr("type") === "button") {
                    if ($(this).hasClass("ui-datepicker-trigger")) {
                        $(this).click();
                    }
                }

                else {
                    //alert($(this).attr('id'));
                    SetFocusonNextElement(this, inputElements, event);

                }
            }
        });
        return this;
    };
} (jQuery));



function SetFocusonNextElement(ele, inputElements, event) {

    var index = 0;

    var nextInput = GetNextFocusElement(ele, inputElements);
    if (null != nextInput) {
//        //for test
//        if ($(nextInput).attr("onblur") != undefined) {
//            alert("onblur exist");
//        }
//        if ($(nextInput).attr("blur") != undefined) {
//            alert("blur exist");
//        }
//        //for test

        nextInput.focus();
        if (null != nextInput.select) {
            nextInput.select();
        }
        return false;
    }
}

function GetNextFocusElement(ele, inputElements) {
    index = inputElements.index(ele);
    index++;
    
    var nextInput = inputElements[index];

    if (nextInput == null) {
        return null;
    }
    if ($(ele).hasClass("date")) {
        if ($(ele).val().length > 0 && $(nextInput).hasClass("ui-datepicker-trigger")) {
            index++;
            nextInput = inputElements[index];
        }
    }

    if (inputElements[index] != null) {
        while ($(nextInput).is(':disabled') || $(nextInput).hasClass('readonly') || (typeof nextInput === "undefined") || !$(nextInput).is(":visible")) {
            index++;
            nextInput = inputElements[index];
            if (nextInput == null) {
                return ele;
            }
        }
        return nextInput;
    }
}


