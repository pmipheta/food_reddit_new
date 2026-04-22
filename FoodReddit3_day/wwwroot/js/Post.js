// ============================================
// Post.js — Fetch Search (no page reload)
// ============================================

// ── State ──
let currentSearch = '';
let currentCommunity = '';
let currentSort = '';
let debounceTimer = null;

// ── Init on DOM ready ──
document.addEventListener('DOMContentLoaded', () => {
    const searchInput = document.querySelector('.Input-search');
    const container = document.getElementById('post-feed-container');

    if (!searchInput || !container) return;

    // Read initial values from URL so state is in sync on first load
    const params = new URLSearchParams(window.location.search);
    currentSearch = params.get('searchQuery') || '';
    currentCommunity = params.get('communityId') || '';
    currentSort = params.get('sortOrder') || '';

    // ── Search input with debounce ──
    searchInput.addEventListener('input', () => {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => {
            currentSearch = searchInput.value.trim();
            fetchPosts();
        }, 350); // wait 350ms after user stops typing
    });

    // ── Prevent form submit (we handle it via fetch) ──
    const searchForm = searchInput.closest('form');
    if (searchForm) {
        searchForm.addEventListener('submit', e => e.preventDefault());
    }

    // ── Category pills ──
    document.querySelectorAll('.cat-pill').forEach(pill => {
        pill.addEventListener('click', e => {
            e.preventDefault();
            document.querySelectorAll('.cat-pill').forEach(p => p.classList.remove('active'));
            pill.classList.add('active');
            currentCommunity = pill.dataset.communityId || '';
            fetchPosts();
        });
    });

    // ── Sort buttons ──
    document.querySelectorAll('.filter-btn').forEach(btn => {
        btn.addEventListener('click', e => {
            e.preventDefault();
            document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            currentSort = btn.dataset.sort || '';
            fetchPosts();
        });
    });
});

// ── Main fetch function ──
async function fetchPosts() {
    const container = document.getElementById('post-feed-container');
    if (!container) return;

    // Build query string
    const params = new URLSearchParams();
    if (currentSearch) params.set('searchQuery', currentSearch);
    if (currentCommunity) params.set('communityId', currentCommunity);
    if (currentSort) params.set('sortOrder', currentSort);

    // Update URL without reload
    const newUrl = params.toString()
        ? `${window.location.pathname}?${params.toString()}`
        : window.location.pathname;
    window.history.pushState({}, '', newUrl);

    // Show loading skeleton
    showLoading(container);

    try {
        const res = await fetch(`/Post/Index?${params.toString()}`, {
            headers: { 'X-Requested-With': 'FetchAPI' }
        });

        if (!res.ok) throw new Error('Network error');

        const html = await res.text();

        // Fade out → swap → fade in
        container.style.opacity = '0';
        container.style.transform = 'translateY(6px)';
        container.style.transition = 'opacity 0.15s ease, transform 0.15s ease';

        setTimeout(() => {
            container.innerHTML = html;
            container.style.opacity = '1';
            container.style.transform = 'translateY(0)';
        }, 150);

    } catch (err) {
        console.error('Fetch error:', err);
        container.innerHTML = `
            <div class="empty-state">
                <div class="empty-icon">⚠️</div>
                <p class="empty-title">Something went wrong</p>
                <p class="empty-message">Please try again</p>
            </div>
        `;
    }
}

// ── Loading skeleton ──
function showLoading(container) {
    const skeletons = Array.from({ length: 3 }, () => `
        <div class="post-card skeleton-card">
            <div class="skeleton-meta"></div>
            <div class="skeleton-title"></div>
            <div class="skeleton-body"></div>
            <div class="skeleton-footer"></div>
        </div>
    `).join('');
    container.innerHTML = skeletons;
    container.style.opacity = '1';
    container.style.transform = 'none';
}

// ── Navbar dropdown helpers ──
function toggleNotif() {
    const d = document.getElementById('notifDropdown');
    const b = document.getElementById('backdrop');
    const open = d?.classList.contains('open');
    closeAll();
    if (!open && d) { d.classList.add('open'); b?.classList.add('active'); }
}

function toggleProfile() {
    const d = document.getElementById('profileDropdown');
    const b = document.getElementById('backdrop');
    const open = d?.classList.contains('open');
    closeAll();
    if (!open && d) { d.classList.add('open'); b?.classList.add('active'); }
}

function closeAll() {
    document.querySelectorAll('.notif-dropdown, .profile-dropdown')
        .forEach(el => el.classList.remove('open'));
    document.getElementById('backdrop')?.classList.remove('active');
}

function clearNotifs() {
    document.querySelectorAll('.notif-item').forEach(el => el.classList.remove('unread'));
    const badge = document.getElementById('notifBadge');
    if (badge) badge.style.display = 'none';
}