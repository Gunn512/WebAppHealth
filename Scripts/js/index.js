//// CAROUSEL VIDEO
//document.addEventListener("DOMContentLoaded", function () {
//  const videoModal = document.getElementById("videoModal");
//  const videoPlayer = document.getElementById("youtubeVideoPlayer");

//  // 1. Khi Modal BẮT ĐẦU MỞ
//  videoModal.addEventListener("show.bs.modal", function (event) {
//    try {
//      // Lấy nút (icon play) đã kích hoạt modal
//      const button = event.relatedTarget;

//      // Lấy link video từ thuộc tính 'data-bs-src'
//      const videoSrc = button.getAttribute("data-bs-src");

//      // (QUAN TRỌNG) Thêm "?autoplay=1" để video tự chạy khi mở
//      videoPlayer.src = videoSrc + "?autoplay=1&mute=1";
//      // (Thêm mute=1 vì nhiều trình duyệt chặn autoplay có tiếng)
//    } catch (e) {
//      console.error("Không thể tải video: ", e);
//    }
//  });

//  // 2. Khi Modal ĐÃ ĐÓNG HOÀN TOÀN
//  videoModal.addEventListener("hidden.bs.modal", function () {
//    // (QUAN TRỌNG) Dừng video bằng cách xóa src
//    videoPlayer.src = "";
//  });
//});

//// CAROUSEL THƯ CẢM ƠN
//document.addEventListener("DOMContentLoaded", function () {
//  const track = document.querySelector(".testimonial-track");
//  if (track) {
//    const nextButton = document.getElementById("testimonial-next");
//    const prevButton = document.getElementById("testimonial-prev");
//    let items = Array.from(track.children);

//    let currentIndex = 0;
//    let isMoving = false;
//    let itemsPerSlide = 3; // Mặc định 3 item

//    // Hàm tính toán lại kích thước và số item
//    function updateSliderConfig() {
//      if (window.innerWidth <= 767) {
//        itemsPerSlide = 1;
//      } else if (window.innerWidth <= 991) {
//        itemsPerSlide = 2;
//      } else {
//        itemsPerSlide = 3;
//      }
//    }

//    // Hàm nhân bản item để tạo vòng lặp vô tận
//    function setupInfiniteLoop() {
//      // Xóa các clone cũ nếu có
//      const oldClones = track.querySelectorAll(".clone");
//      oldClones.forEach((clone) => clone.remove());

//      // Lấy lại danh sách item gốc
//      items = Array.from(track.children).filter(
//        (el) => !el.classList.contains("clone")
//      );

//      // Clone các item đầu và cuối
//      const clonesStart = items
//        .slice(items.length - itemsPerSlide)
//        .map((item) => item.cloneNode(true));
//      const clonesEnd = items
//        .slice(0, itemsPerSlide)
//        .map((item) => item.cloneNode(true));

//      clonesStart.forEach((clone) => {
//        clone.classList.add("clone");
//        track.prepend(clone);
//      });
//      clonesEnd.forEach((clone) => {
//        clone.classList.add("clone");
//        track.append(clone);
//      });

//      // Cập nhật lại danh sách items bao gồm cả clone
//      items = Array.from(track.children);
//    }

//    // Hàm di chuyển (không có hiệu ứng)
//    function resetPosition() {
//      isMoving = true;
//      track.style.transition = "none";

//      // Tính toán vị trí bắt đầu (sau các clone đầu)
//      currentIndex = itemsPerSlide;
//      const itemWidth = track.scrollWidth / items.length;
//      const initialOffset = -(currentIndex * itemWidth);
//      track.style.transform = `translateX(${initialOffset}px)`;

//      setTimeout(() => {
//        isMoving = false;
//      }, 50);
//    }

//    // Hàm chính: di chuyển slider
//    function moveSlider(direction) {
//      if (isMoving) return;
//      isMoving = true;

//      // Bật hiệu ứng
//      track.style.transition = "transform 0.5s ease";

//      // Tăng/giảm index
//      currentIndex += direction;

//      // Tính toán vị trí mới
//      const itemWidth = track.scrollWidth / items.length;
//      const newOffset = -(currentIndex * itemWidth);
//      track.style.transform = `translateX(${newOffset}px)`;
//    }

//    // Lắng nghe khi hiệu ứng trượt kết thúc
//    track.addEventListener("transitionend", () => {
//      isMoving = false;

//      // Lấy tổng số item gốc (không clone)
//      const realItemsCount = items.length - 2 * itemsPerSlide;

//      // Nếu đi đến cuối (đang ở clone cuối)
//      if (currentIndex >= realItemsCount + itemsPerSlide) {
//        track.style.transition = "none";
//        currentIndex = itemsPerSlide; // Quay về item 1
//        const itemWidth = track.scrollWidth / items.length;
//        const newOffset = -(currentIndex * itemWidth);
//        track.style.transform = `translateX(${newOffset}px)`;
//      }

//      // Nếu đi về đầu (đang ở clone đầu)
//      if (currentIndex < itemsPerSlide) {
//        track.style.transition = "none";
//        currentIndex = realItemsCount + itemsPerSlide - 1; // Quay về item cuối
//        const itemWidth = track.scrollWidth / items.length;
//        const newOffset = -(currentIndex * itemWidth);
//        track.style.transform = `translateX(${newOffset}px)`;
//      }
//    });

//    // Gắn sự kiện click
//    nextButton.addEventListener("click", () => moveSlider(1)); // 1 = next
//    prevButton.addEventListener("click", () => moveSlider(-1)); // -1 = prev

//    // Tự động chạy (thêm vào)
//    setInterval(() => {
//      if (!isMoving) {
//        // Chỉ tự chạy khi user không tương tác
//        moveSlider(1);
//      }
//    }, 4000); // Đổi 3000ms (3 giây) thành tốc độ bạn muốn

//    // Thiết lập ban đầu
//    updateSliderConfig();
//    setupInfiniteLoop();
//    resetPosition();

//    // Cập nhật lại khi thay đổi kích thước cửa sổ
//    window.addEventListener("resize", () => {
//      updateSliderConfig();
//      setupInfiniteLoop();
//      resetPosition();
//    });
//  }
//});
