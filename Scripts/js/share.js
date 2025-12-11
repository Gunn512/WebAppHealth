// --- HÀM XỬ LÝ LOGIC STICKY NAVBAR ---
function initStickyNavbar(navbar) {
  // 1. Tạo div giả để giữ chỗ (tránh giật layout khi navbar thành fixed)
  const placeholder = document.createElement("div");
  placeholder.className = "navbar-dummy-placeholder";
  placeholder.style.display = "none";
  navbar.parentNode.insertBefore(placeholder, navbar);

  // 2. Các biến trạng thái
  let isSticky = false;
  let navbarOffsetTop = navbar.offsetTop;

  // Cập nhật lại vị trí nếu cửa sổ thay đổi kích thước (đề phòng header bên trên thay đổi chiều cao)
  window.addEventListener("resize", function () {
    navbarOffsetTop = navbar.offsetTop;
  });

  // 3. Sự kiện cuộn trang
  window.addEventListener("scroll", function () {
    const scrollY = window.scrollY || document.documentElement.scrollTop;
    const navbarHeight = navbar.offsetHeight;

    // Cập nhật chiều cao div giả
    placeholder.style.height = navbarHeight + "px";

    // Logic thêm/bớt class
    if (scrollY >= navbarOffsetTop) {
      if (!isSticky) {
        navbar.classList.add("sticky-navbar-active");
        placeholder.style.display = "block";
        isSticky = true;
      }
    } else {
      if (isSticky) {
        navbar.classList.remove("sticky-navbar-active");
        placeholder.style.display = "none";
        isSticky = false;
      }
    }
  });
}
