'use strict';

function postToDatabase(postData, functionPath) {

    fetch(functionPath, 
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
        if (functionPath != '/cart/AddToCart') {
            updateCart();
        }       
    })
}

function updateCart() {
    let cartWrap = document.querySelector('.cartWrap');
    fetch('/cart/CartItems', { credentials: "same-origin" })
    .then(function (response) { return response.text(); })
    .then(function (result) {
        cartWrap.innerHTML = result;
        addDeleteEventListener();
        addUpdateEventListener();
    })
}

let addButtons = document.querySelectorAll('.addProduct');

addButtons.forEach(function (button) {
    button.addEventListener('click', function (event) {
        event.preventDefault();
        let postData = new FormData();
        postData.append('productId', button.parentElement.querySelector('.productId').value);
        postData.append('orderQty', button.parentElement.querySelector('.addQty').value);

        postToDatabase(postData, '/cart/AddToCart');
    })
})

function addDeleteEventListener() {
    let deleteButtons = document.querySelectorAll('.delete');
    deleteButtons.forEach(function (button) {
        button.addEventListener('click', function (event) {
            event.preventDefault();
            let postData = new FormData();
            postData.append('productId', button.parentElement.querySelector('.productId').value);
            postData.append('cartId', button.parentElement.querySelector('.cartId').value);

            postToDatabase(postData, '/cart/RemoveFromCart');
        })
    })   
}
addDeleteEventListener();


function addUpdateEventListener(button) {
    let updateButtons = document.querySelectorAll('.updateQty');
    updateButtons.forEach(function (button) {
        button.addEventListener('click', function (event) {
            event.preventDefault();
            let postData = new FormData();
            postData.append('productId', button.parentElement.querySelector('.productId').value);
            postData.append('cartId', button.parentElement.querySelector('.cartId').value);
            postData.append('qty', button.parentElement.querySelector('.qty').value);

            postToDatabase(postData, '/cart/UpdateProductQty');
        })
    })
}
addUpdateEventListener();