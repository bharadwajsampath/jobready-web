
$(document).ready(function () {
    $('.btnmodal').leanModal({
        ready: function () {
            GetAttendanceDetails();
        },
        //complete: () => { alert('complete');}
    });
    $('select').material_select();
    $('.progress').hide();
    $('.progress-spinner').hide();
    checkAttendanceSwitch();
});

$('#chkattendance').click(function () {
    checkAttendanceSwitch(this);
});


function checkAttendanceSwitch(event) {
    var chk = $(event);
    if (chk.attr('checked')) {
        chk.attr('checked', false);
        $('.absent').show();
        $('.present').hide();
    }
    else {
        chk.attr('checked', true);
        $('.absent').hide();
        $('.present').show();
    }
}
var dataFromModal = {};
$('.btnmodal').click(function () {
    var data = {};
    var fullname = $(this).data('fullname');
    data.fullname = fullname;
    data.coursename = $(this).data('courseid');
    data.eventid = $(this).data('eventid');
    data.partyid = $(this).data('partyid');
    $('#studentname').html(fullname.toUpperCase());
    dataFromModal = data;
});

function GetAttendanceDetails() {
    var requestData = dataFromModal;
    $('.progress').show();
    $.get('/Event/GetDefaultAttendance', requestData, function (response) {
       // debugger;
       // $("#intime option").filter(function (index) { return $(this).text() === response.arrivedAt; }).attr('selected', 'selected');

        // $("#intime option:contains(" + response.arrivedAt + ")").attr('selected', 'selected');
       // $("#outtime option:contains(" + response.leftAt + ")").attr('selected', 'selected');

        $('#absentreason').val(response.absentreason);
        $('#attendancenotes').val(response.notes);
        $('.progress').hide();
    });
}


//Gather the details to be posted
function PostAttendanceDetails() {
    $('.progress-spinner').show();
    var data = {};
    var ispresent = $('#chkattendance').attr('checked');
    if (ispresent) {
        data.attended = true;
        data.intime = $('#intime').val();
        data.outtime = $('#outtime').val();
    }
    else {
        data.attended = false;
        data.absentreason = $('#absentreason option:selected').text();
    }
    data.notes = $('#attendancenotes').val();
    data.coursenumber = dataFromModal.coursename;
    data.eventid = dataFromModal.eventid;
    data.partyid = dataFromModal.partyid;
    $.post('/event/MarkAttendance', data, function (response) {
        if (response === "NegativeTime") {
            Materialize.toast('Negative Duration selected for ' + dataFromModal.fullname, 4000, '', function () { $('.btnmodal').click(); })

        }
        else {
            Materialize.toast('Attendance Marked for ' + dataFromModal.fullname, 4000);
            location.reload();
            //var classListData = {};
            //classListData.courseName = data.coursenumber;
            //classListData.eventId = data.eventid;
            //debugger;
            //$.get('/event/ClassList', classListData);
        }

        $('.progress-spinner').hide();
    });

}


$('#markattendancebtn').click(function () {
    PostAttendanceDetails();
});


