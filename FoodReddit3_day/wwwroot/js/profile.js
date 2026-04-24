async function deletePost(event, postId) {
    event.preventDefault();
    event.stopPropagation();

    

    try {
        const response = await fetch(`/Profile/Delete?id=${postId}`, {
            method: 'POST',
            headers: {
                'X-Requested-With': 'FetchAPI'
            }
        });

        const data = await response.json();

        if (data.success) {
            const postCard = document.getElementById(`post-card-${postId}`);
            if (postCard) {
                postCard.style.opacity = '0';
                postCard.style.transform = 'scale(0.9)';
                postCard.style.transition = '0.2s ease';

                setTimeout(() => {
                    postCard.remove();

                    const container = document.querySelector('.post-list');
                    if (container && container.querySelectorAll('.post-card').length === 0) {
                        container.innerHTML = `
                            <div class="empty-state">
                                <span class="empty-icon">🍳</span>
                                <p class="empty-title">No recipes left</p>
                                <p class="empty-sub">Share a new recipe with the community!</p>
                            </div>
                        `;
                    }
                }, 300);
            }
        } else {
            
            console(data.message);
        }
    } catch (error) {
        console.error('Error:', error);
        
        console('Could not delete the post. Please try again.');
    }
}