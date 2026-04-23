
(function () {
    const POLL_INTERVAL = 30000; 

    let badge, dropdown, list; 

    // ── Icon map ──
    const icons = {
        new_recipe: '🍳',
        comment: '💬',
        upvote: '▲',
        follow: '👤',
        default: '🔔'
    };

   
    document.addEventListener('DOMContentLoaded', () => {
        badge = document.getElementById('notifBadge');
        dropdown = document.getElementById('notifDropdown');
        list = dropdown ? dropdown.querySelector('.notif-list') : null;

        if (!dropdown) return; 

        
        fetchNotifications();
        setInterval(fetchNotifications, POLL_INTERVAL);
    });

   
    async function fetchNotifications() {
        if (!dropdown) return;
        try {
            const res = await fetch('/Notification/GetMyNotifications');
            const data = await res.json();
            if (!data.success) return;

            renderNotifications(data.notifications, data.unreadCount);
        } catch (e) {
            console.warn('Notification fetch failed:', e);
        }
    }

    // ── Render into dropdown ──
    function renderNotifications(notifs, unreadCount) {
        if (badge) {
            if (unreadCount > 0) {
                badge.textContent = unreadCount > 99 ? '99+' : unreadCount;
                badge.style.display = 'flex';
            } else {
                badge.style.display = 'none';
            }
        }

        if (!list) return;

        if (notifs.length === 0) {
            list.innerHTML = `
                <div style="padding: 28px 16px; text-align:center; color:#b5a898; font-size:13px;">
                    🍽️<br>No notifications yet
                </div>`;
            return;
        }

        list.innerHTML = notifs.map(n => {
            const icon = icons[n.type] || icons.default;
            const unread = !n.isRead;
            const postUrl = n.postId ? `/Post/Details/${n.postId}` : '#';

            return `
            <div class="notif-item ${unread ? 'unread' : ''}"
                 data-id="${n.id}"
                 onclick="notifClick(${n.id}, '${postUrl}')">
                <span class="notif-dot ${unread ? '' : 'read'}"></span>
                <div class="notif-content">
                    <p class="notif-text">
                        <span style="font-size:14px; margin-right:4px;">${icon}</span>
                        ${escapeHtml(n.message)}
                    </p>
                    <span class="notif-time">${n.timeAgo}</span>
                </div>
            </div>`;
        }).join('');
    }

    window.notifClick = async function (id, url) {
        await fetch(`/Notification/MarkRead/${id}`, { method: 'POST' });
        if (url && url !== '#') window.location.href = url;
        fetchNotifications();
    };

    // ── Clear all ──
    window.clearNotifs = async function () {
        
        if (badge) badge.style.display = 'none';

        document.querySelectorAll('.notif-item.unread').forEach(item => {
            item.classList.remove('unread');
            const dot = item.querySelector('.notif-dot');
            if (dot) {
                dot.classList.remove('unread');
                dot.classList.add('read');
            }
        });

        try {
            
            await fetch('/Notification/MarkAllRead', { method: 'POST' });

            
            fetchNotifications();
        } catch (err) {
            console.error("Error clearing notifications:", err);
        }
    };

    // ── Toggle dropdown ──
    window.toggleNotif = function () {
        const backdrop = document.getElementById('backdrop');
        const isOpen = dropdown.classList.contains('open');
        window.closeAllNotifs();
        if (!isOpen) {
            dropdown.classList.add('open');
            backdrop?.classList.add('active');
            fetchNotifications();
        }
    };

    window.closeAllNotifs = function () {
        document.querySelectorAll('.notif-dropdown, .profile-dropdown')
            .forEach(el => el.classList.remove('open'));
        document.getElementById('backdrop')?.classList.remove('active');
    }

    // ── Helper: escape HTML ──
    function escapeHtml(str) {
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }
})();