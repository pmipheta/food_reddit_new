function votePost(postId, voteValue) {
    fetch('/Post/Vote', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: `postId=${postId}&value=${voteValue}`
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // ❌ ลบ window.location.reload(); ออกไปแล้ว

                // 🌟 1. ดึง Elements ของคะแนนและปุ่มลูกศรมา
                const scoreEl = document.getElementById(`score-${postId}`);
                const upBtn = document.getElementById(`upvote-${postId}`);
                const downBtn = document.getElementById(`downvote-${postId}`);

                // 🌟 2. อัปเดตตัวเลขคะแนนแบบ Real-time
                if (scoreEl) {
                    scoreEl.innerText = data.newScore;
                    // เอนิเมชันเด้งดึ๋งเล็กน้อยตอนคะแนนเปลี่ยน
                    scoreEl.style.transform = "scale(1.2)";
                    setTimeout(() => scoreEl.style.transform = "scale(1)", 200);
                }

                // 🌟 3. ลอจิกการเปลี่ยนสีปุ่ม
                if (upBtn && downBtn) {
                    // เช็คว่าตอนที่เรากดปุ่มนั้น ปุ่มมันมีสีอยู่แล้วหรือเปล่า (ถ้ามีแปลว่าเรากำลัง "กดยกเลิกโหวต")
                    let isCancelling = false;
                    if (voteValue === 1 && upBtn.classList.contains('upvoted')) isCancelling = true;
                    if (voteValue === -1 && downBtn.classList.contains('downvoted')) isCancelling = true;

                    // เคลียร์สีปุ่มทั้งหมดทิ้งก่อน
                    upBtn.classList.remove('upvoted');
                    downBtn.classList.remove('downvoted');
                    if (scoreEl) scoreEl.classList.remove('upvoted', 'downvoted');

                    // ถ้าไม่ได้กดยกเลิก ก็ให้ใส่สีใหม่เข้าไปตามที่เรากด
                    if (!isCancelling) {
                        if (voteValue === 1) {
                            upBtn.classList.add('upvoted');
                            if (scoreEl) scoreEl.classList.add('upvoted');
                        } else {
                            downBtn.classList.add('downvoted');
                            if (scoreEl) scoreEl.classList.add('downvoted');
                        }
                    }
                }

            } else {
                if (data.message === "Please login first") {
                    alert("กรุณาล็อกอินก่อนทำการโหวตครับ!");
                    window.location.href = '/User/Login';
                } else {
                    alert("เกิดข้อผิดพลาด: " + data.message);
                }
            }
        })
        .catch(error => console.error('Error:', error));
}
function submitMainComment(event, postId) {
    // 1. ป้องกันไม่ให้หน้าเว็บรีเฟรชตอนกดปุ่ม Submit
    event.preventDefault();

    const textarea = document.getElementById('main-comment-body');
    const bodyText = textarea.value;

    // 2. ส่งข้อมูลไปที่ Controller
    fetch('/Post/AddComment', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        // ใช้ encodeURIComponent เพื่อป้องกันบั๊กเวลาคนพิมพ์เครื่องหมายแปลกๆ (เช่น & หรือ ?)
        body: `postId=${postId}&body=${encodeURIComponent(bodyText)}`
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // 3. เคลียร์ข้อความในกล่อง textarea ให้ว่างเปล่า
                textarea.value = '';

                // 4. สร้างกล่อง HTML ของคอมเมนต์ใหม่ (ผมตกแต่งกรอบสีส้มให้รู้ว่าเป็นคอมเมนต์ใหม่ด้วยครับ)
                const newCommentHtml = `
                    <div style="background-color: #fffaf5; padding: 12px; margin-bottom: 15px; border-left: 4px solid var(--orange, #e8520a); border-radius: 4px;">
                        <p style="margin: 0 0 8px 0;">
                            <strong>${data.username}</strong> 
                            <span style="color: gray; font-size: 13px;"> / ${data.date} (Just now)</span>
                        </p>
                        <p style="margin: 0;">${data.text}</p>
                    </div>
                `;

                // 5. เอาคอมเมนต์ใหม่ไปแปะไว้ด้านบนสุดของรายการ
                const commentList = document.getElementById('all-comments-list');
                commentList.insertAdjacentHTML('afterbegin', newCommentHtml);

            } else {
                if (data.message === "Please login first") {
                    alert("กรุณาล็อกอินก่อนคอมเมนต์ครับ!");
                    window.location.href = '/User/Login';
                } else {
                    alert("Error: " + data.message);
                }
            }
        })
        .catch(error => console.error('Error:', error));
}