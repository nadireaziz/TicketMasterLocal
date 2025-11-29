const API_BASE = 'http://localhost:5277/api';

// Get auth token
function getToken() {
    return localStorage.getItem('accessToken');
}

// Show alert
function showAlert(message, type = 'info') {
    const alertBox = document.getElementById('alertBox');
    alertBox.textContent = message;
    alertBox.className = `alert alert-${type}`;
    alertBox.style.display = 'block';

    setTimeout(() => {
        alertBox.style.display = 'none';
    }, 4000);
}

// Check authentication
function checkAuth() {
    const token = getToken();
    const user = JSON.parse(localStorage.getItem('user') || '{}');

    if (!token || !user.userId) {
        window.location.href = '/';
        return null;
    }

    return user;
}

// Handle logout
async function handleLogout() {
    const refreshToken = localStorage.getItem('refreshToken');

    try {
        await fetch(`${API_BASE}/auth/logout`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${getToken()}`
            },
            body: JSON.stringify({ refreshToken })
        });
    } catch (error) {
        console.error('Logout error:', error);
    }

    localStorage.clear();
    window.location.href = '/';
}

// Tab switching
function showTab(tabName) {
    // Hide all tabs
    document.querySelectorAll('.tab-content').forEach(tab => {
        tab.classList.remove('active');
    });

    // Remove active from all buttons
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.classList.remove('active');
    });

    // Show selected tab
    document.getElementById(`${tabName}Tab`).classList.add('active');
    event.target.classList.add('active');
}

// Fetch events
async function fetchEvents() {
    try {
        const response = await fetch(`${API_BASE}/events`);
        const events = await response.json();

        const eventsList = document.getElementById('eventsList');

        if (events.length === 0) {
            eventsList.innerHTML = '<div class="empty-state"><h3>No Events Available</h3><p>Check back later for upcoming events</p></div>';
            return;
        }

        eventsList.innerHTML = events.map(event => `
            <div class="card">
                <div class="card-header">
                    <h3>${event.name}</h3>
                    <p>${event.category.name}</p>
                </div>
                <div class="card-body">
                    <div class="card-info">
                        <div class="info-row">
                            <span class="info-label">Venue</span>
                            <span class="info-value">${event.venue.name}</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">City</span>
                            <span class="info-value">${event.venue.city}</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">Date</span>
                            <span class="info-value">${new Date(event.eventDate).toLocaleDateString()}</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">Base Price</span>
                            <span class="info-value">$${event.basePrice.toFixed(2)}</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">Status</span>
                            <span class="badge badge-success">${event.status}</span>
                        </div>
                    </div>
                </div>
            </div>
        `).join('');
    } catch (error) {
        console.error('Error fetching events:', error);
        document.getElementById('eventsList').innerHTML = '<div class="empty-state"><h3>Error loading events</h3></div>';
    }
}

// Fetch bookings
async function fetchBookings() {
    try {
        const response = await fetch(`${API_BASE}/bookings/my-bookings`, {
            headers: {
                'Authorization': `Bearer ${getToken()}`
            }
        });

        if (!response.ok) {
            throw new Error('Failed to fetch bookings');
        }

        const bookings = await response.json();
        const bookingsList = document.getElementById('bookingsList');

        if (bookings.length === 0) {
            bookingsList.innerHTML = '<div class="empty-state"><h3>No Bookings Yet</h3><p>Start booking tickets to see them here</p></div>';
            return;
        }

        bookingsList.innerHTML = bookings.map(booking => `
            <div class="card">
                <div class="card-header">
                    <h3>${booking.event.name}</h3>
                    <p>Ref: ${booking.bookingReference}</p>
                </div>
                <div class="card-body">
                    <div class="card-info">
                        <div class="info-row">
                            <span class="info-label">Event Date</span>
                            <span class="info-value">${new Date(booking.event.eventDate).toLocaleDateString()}</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">Booking Date</span>
                            <span class="info-value">${new Date(booking.bookingDate).toLocaleDateString()}</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">Total Amount</span>
                            <span class="info-value">$${booking.totalAmount.toFixed(2)}</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">Tickets</span>
                            <span class="info-value">${booking.ticketCount}</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">Status</span>
                            <span class="badge badge-${booking.status === 'Confirmed' ? 'success' : 'warning'}">${booking.status}</span>
                        </div>
                    </div>
                </div>
            </div>
        `).join('');
    } catch (error) {
        console.error('Error fetching bookings:', error);
        document.getElementById('bookingsList').innerHTML = '<div class="empty-state"><h3>Error loading bookings</h3></div>';
    }
}

// Fetch venues
async function fetchVenues() {
    try {
        const response = await fetch(`${API_BASE}/venues`);
        const venues = await response.json();

        const venuesList = document.getElementById('venuesList');

        if (venues.length === 0) {
            venuesList.innerHTML = '<div class="empty-state"><h3>No Venues Available</h3></div>';
            return;
        }

        venuesList.innerHTML = venues.map(venue => `
            <div class="card">
                <div class="card-header">
                    <h3>${venue.name}</h3>
                    <p>${venue.city}</p>
                </div>
                <div class="card-body">
                    <div class="card-info">
                        <div class="info-row">
                            <span class="info-label">Capacity</span>
                            <span class="info-value">${venue.capacity.toLocaleString()}</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">Events</span>
                            <span class="info-value">${venue.eventCount}</span>
                        </div>
                        <div class="info-row">
                            <span class="info-label">Sections</span>
                            <span class="info-value">${venue.sectionCount}</span>
                        </div>
                    </div>
                </div>
            </div>
        `).join('');
    } catch (error) {
        console.error('Error fetching venues:', error);
        document.getElementById('venuesList').innerHTML = '<div class="empty-state"><h3>Error loading venues</h3></div>';
    }
}

// Fetch session count
async function fetchSessionCount() {
    try {
        const response = await fetch(`${API_BASE}/auth/sessions`, {
            headers: {
                'Authorization': `Bearer ${getToken()}`
            }
        });

        const data = await response.json();
        document.getElementById('sessionCount').textContent = data.activeSessions;
    } catch (error) {
        console.error('Error fetching session count:', error);
        document.getElementById('sessionCount').textContent = 'N/A';
    }
}

// Initialize dashboard
window.addEventListener('DOMContentLoaded', () => {
    const user = checkAuth();

    if (user) {
        // Display user info
        document.getElementById('userName').textContent = user.email;
        document.getElementById('userFullName').textContent = user.fullName;
        document.getElementById('userRole').textContent = user.role;

        // Fetch data
        fetchEvents();
        fetchBookings();
        fetchVenues();
        fetchSessionCount();
    }
});
