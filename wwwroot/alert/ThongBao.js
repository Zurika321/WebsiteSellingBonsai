// Tạo đối tượng ThongBao với các phương thức như Primary, Success, Danger
const ThongBao = {
    // Phương thức tạo thông báo loại Primary
    Primary: function (thong_bao, x) {
        thong_bao_chung(thong_bao, x, 'Primary');
    },
    // Phương thức tạo thông báo loại Success
    Success: function (thong_bao, x) {
        thong_bao_chung(thong_bao, x, 'Success');
    },
    // Phương thức tạo thông báo loại Danger
    Danger: function (thong_bao, x) {
        thong_bao_chung(thong_bao, x, 'Danger');
    },
    // Phương thức tạo thông báo loại Warning
    Warning: function (thong_bao, x) {
        thong_bao_chung(thong_bao, x, 'Warning');
    }
};

const Icon = {
    Primary: '<i class="fa-regular fa-circle-info"></i>',
    Success: '<i class="fa-regular fa-circle-check"></i>',
    Danger: '<i class="fa-regular fa-circle-exclamation"></i>',
    Warning: '<i class="fa-regular fa-triangle-exclamation"></i>'
};

// Hàm chung để tạo thông báo
function thong_bao_chung(thong_bao, x, className) {
    if (x === undefined) {
        x = 2; // Thời gian mặc định là 2 giây
    }

    // Tạo div thông báo
    let div = document.createElement("div");
    div.classList.add("thong_bao_chung", "text-light", "alert", "alert-" + className.toLowerCase());
    div.innerHTML = Icon[className.trim()] + " " + thong_bao;

    // Tạo nút xóa thông báo
    let remove_thong_bao = document.createElement("div");
    remove_thong_bao.className = "remove_thong_bao";
    remove_thong_bao.textContent = "X";
    remove_thong_bao.addEventListener("click", () => {
        if (document.body.contains(div)) {
            document.body.removeChild(div);
        }
    });

    // Thêm nút xóa vào thông báo
    div.appendChild(remove_thong_bao);

    // Thêm thông báo vào body hoặc một phần tử khác nếu không tìm thấy body
    var targetElement = document.body;

    if (targetElement == null) {
        document.addEventListener("DOMContentLoaded", function () {
            targetElement = document.body;
            targetElement.appendChild(div);
        });
    } else {
        targetElement.appendChild(div);
    }
    

    // Cài đặt hiệu ứng cho thông báo
    div.offsetHeight;  // Force reflow to trigger transition
    div.style.top = "20px";  // Vị trí ban đầu
    div.style.opacity = "1";  // Làm cho thông báo hiện lên

    // Thời gian hiển thị thông báo
    setTimeout(() => {
        div.style.opacity = "0";  // Làm cho thông báo mờ đi sau x giây
        setTimeout(() => {
            if (targetElement.contains(div)) {
                targetElement.removeChild(div);  // Xóa thông báo khỏi phần tử
            }
        }, 1000);  // Thời gian mờ đi
    }, 1000 * x);  // Thời gian hiển thị thông báo trước khi mờ đi
}
