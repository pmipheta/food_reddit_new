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
                

                
                const scoreEl = document.getElementById(`score-${postId}`);
                const upBtn = document.getElementById(`upvote-${postId}`);
                const downBtn = document.getElementById(`downvote-${postId}`);

                
                if (scoreEl) {
                    scoreEl.innerText = data.newScore;
                    
                    scoreEl.style.transform = "scale(1.2)";
                    setTimeout(() => scoreEl.style.transform = "scale(1)", 200);
                }

                
                if (upBtn && downBtn) {
                    
                    let isCancelling = false;
                    if (voteValue === 1 && upBtn.classList.contains('upvoted')) isCancelling = true;
                    if (voteValue === -1 && downBtn.classList.contains('downvoted')) isCancelling = true;

                    
                    upBtn.classList.remove('upvoted');
                    downBtn.classList.remove('downvoted');
                    if (scoreEl) scoreEl.classList.remove('upvoted', 'downvoted');

                    
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
                    console("Login before Vote");
                    window.location.href = '/User/Login';
                } else {
                    console("Error: " + data.message);
                }
            }
        })
        .catch(error => console.error('Error:', error));
}
function submitMainComment(event, postId) {
    
    event.preventDefault();

    const textarea = document.getElementById('main-comment-body');
    const bodyText = textarea.value;

    
    fetch('/Post/AddComment', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        
        body: `postId=${postId}&body=${encodeURIComponent(bodyText)}`
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                
                textarea.value = '';

                
                const newCommentHtml = `
                    <div style="background-color: #fffaf5; padding: 12px; margin-bottom: 15px; border-left: 4px solid var(--orange, #e8520a); border-radius: 4px;">
                        <p style="margin: 0 0 8px 0;">
                            <strong>${data.username}</strong> 
                            <span style="color: gray; font-size: 13px;"> / ${data.date} (Just now)</span>
                        </p>
                        <p style="margin: 0;">${data.text}</p>
                    </div>
                `;

                
                const commentList = document.getElementById('all-comments-list');
                commentList.insertAdjacentHTML('afterbegin', newCommentHtml);

            } else {
                if (data.message === "Please login first") {
                    console("Login before comment!");
                    window.location.href = '/User/Login';
                } else {
                    console("Error: " + data.message);
                }
            }
        })
        .catch(error => console.error('Error:', error));
}