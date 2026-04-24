
function votePost(postId, voteValue) {
    fetch('/Post/Vote', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `postId=${postId}&value=${voteValue}`
    })
        .then(r => r.json())
        .then(data => {
            if (!data.success) {
                if (data.message === 'Please login first') window.location.href = '/User/Login';
                else console.error('Vote error:', data.message);
                return;
            }
            const scoreEl = document.getElementById(`score-${postId}`);
            const upBtn = document.getElementById(`upvote-${postId}`);
            const downBtn = document.getElementById(`downvote-${postId}`);

            if (scoreEl) {
                scoreEl.innerText = data.newScore;
                scoreEl.style.transform = 'scale(1.3)';
                setTimeout(() => scoreEl.style.transform = 'scale(1)', 200);
            }

            if (upBtn && downBtn) {
                const cancelling =
                    (voteValue === 1 && upBtn.classList.contains('upvoted')) ||
                    (voteValue === -1 && downBtn.classList.contains('downvoted'));

                upBtn.classList.remove('upvoted');
                downBtn.classList.remove('downvoted');
                scoreEl?.classList.remove('upvoted', 'downvoted');

                if (!cancelling) {
                    if (voteValue === 1) {
                        upBtn.classList.add('upvoted');
                        scoreEl?.classList.add('upvoted');
                    } else {
                        downBtn.classList.add('downvoted');
                        scoreEl?.classList.add('downvoted');
                    }
                }
            }
        })
        .catch(err => console.error('Fetch error:', err));
}

// ── Main comment ──────────────────────────────────────────────
function submitMainComment(event, postId) {
    event.preventDefault();
    const textarea = document.getElementById('main-comment-body');
    const bodyText = textarea.value.trim();
    if (!bodyText) return;

    fetch('/Post/AddComment', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `postId=${postId}&body=${encodeURIComponent(bodyText)}`
    })
        .then(r => r.json())
        .then(data => {
            if (!data.success) {
                if (data.message === 'Please login first') window.location.href = '/User/Login';
                else console.error('Comment error:', data.message);
                return;
            }
            textarea.value = '';
            document.getElementById('all-comments-list')
                .insertAdjacentHTML('afterbegin', buildCommentHTML(data, postId));
        })
        .catch(err => console.error('Fetch error:', err));
}


document.addEventListener('submit', function (event) {
    const form = event.target;
    const parentInput = form.querySelector('input[name="parentCommentId"]');
    if (!parentInput) return;      
    event.preventDefault();

    const postId = form.querySelector('input[name="postId"]').value;
    const parentCommentId = parentInput.value;
    const input = form.querySelector('input[name="body"], textarea[name="body"]');
    const bodyText = input?.value.trim();
    if (!bodyText) return;

    fetch('/Post/AddComment', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `postId=${encodeURIComponent(postId)}&body=${encodeURIComponent(bodyText)}&parentCommentId=${encodeURIComponent(parentCommentId)}`
    })
        .then(r => r.json())
        .then(data => {
            if (!data.success) {
                if (data.message === 'Please login first') window.location.href = '/User/Login';
                else console.error('Reply error:', data.message);
                return;
            }
            input.value = '';

            
            const commentItem = form.closest('.comment-item');
            let repliesContainer = commentItem?.querySelector('.replies-container');
            if (!repliesContainer) {
                repliesContainer = document.createElement('div');
                repliesContainer.className = 'replies-container';
                form.after(repliesContainer);
            }
            repliesContainer.insertAdjacentHTML('beforeend', buildReplyHTML(data));
        })
        .catch(err => console.error('Fetch error:', err));
});


function buildCommentHTML(data, postId) {
    const cId = data.commentId ?? '';
    const letter = (data.username || '?')[0].toUpperCase();
    return `
<div class="comment-item" data-comment-id="${cId}">
    <div class="comment-meta">
        <div class="comment-avatar">${letter}</div>
        <strong class="comment-author">${escapeHtml(data.username)}</strong>
        <span class="comment-time">· ${data.date} · just now</span>
    </div>
    <p class="comment-text">${escapeHtml(data.text)}</p>
    <form class="reply-form">
        <input type="hidden" name="postId"          value="${postId}" />
        <input type="hidden" name="parentCommentId" value="${cId}" />
        <input type="text"   name="body" placeholder="Write a reply…" />
        <button type="submit">Reply</button>
    </form>
    <div class="replies-container"></div>
</div>`;
}

function buildReplyHTML(data) {
    const letter = (data.username || '?')[0].toUpperCase();
    return `
<div class="reply-item">
    <div class="comment-meta">
        <div class="comment-avatar">${letter}</div>
        <strong class="comment-author">${escapeHtml(data.username)}</strong>
        <span class="comment-time">· ${data.date} · just now</span>
    </div>
    <p class="comment-text">${escapeHtml(data.text)}</p>
</div>`;
}

function escapeHtml(str) {
    return String(str)
        .replace(/&/g, '&amp;').replace(/</g, '&lt;')
        .replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}