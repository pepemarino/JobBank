var common = common || {}; // old school declaring the namespace

$(function () {
    common.initScrolling();
});

common.initScrolling = function () {
    window.scrollToBottom = (elementId) => {
        const el = document.getElementById(elementId);
        if (el) {
            el.scrollTop = el.scrollHeight;
        }
    };
};