$(function () {

    $(".max-people").on('change', setButtonValidity);
    $(".date-picker").on('change', setButtonValidity);

    function setButtonValidity() {
        $(".submit-button").prop('disabled', !isFormValid());
    }

    function isFormValid() {
        var selectedValue = $(".max-people option:selected").val();
        var date = $(".date-picker").val();
        console.log(date);
        return selectedValue !== "0" && date;
    }
});