'use strict';

let addButtons = document.querySelectorAll('.addProduct');

addButtons.forEach(function (button) {
    button.addEventListener('click', function(event) {
        event.preventDefault();
        let productId = button.parentElement.querySelector('.productId').value;
        let qty = button.parentElement.querySelector('.addQty').value;
        postToDatabase(productId, qty);
    })
})

function postToDatabase(productId, qty) {

    let postData = new FormData();
    postData.append("productId", productId);
    postData.append("orderQty", qty);
    fetch('/cart/AddToCart', 
    {
        method: "POST",
        body: postData,
        credentials: "same-origin",
    })
    .then(function (response) { return response.json();})
    .then(function (result) {
        let message = document.querySelector('.message');
        if (result.success) {
            message.classList.add('ok');
            setTimeout(function () {
                message.classList.remove('ok');
            }, 5000);
        } else {
            message.classList.add('error');
            setTimeout(function () {
                message.classList.remove('error');
            }, 5000);
        }
        document.querySelector('.message p').innerHTML = result.message;
    })
}