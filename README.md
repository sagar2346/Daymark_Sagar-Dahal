# DayMark — Premium Personal Journaling System 📝

A high-performance, secure, and feature-rich cross-platform journaling application built with **.NET MAUI** and **SQLite**. Designed for users who value privacy, rich-text expression, and data-driven personal growth.

## 🌟 Key Features

- **🔐 Secure Authentication:** Password-protected entries with setup and login modes.
- **✍️ Markdown Editor:** Rich-text support with live HTML preview rendering.
- **📊 Advanced Analytics:** Interactive charts for mood distribution, word count trends, and time patterns.
- **🔥 Persistence & Habits:** Automatic journaling streak tracking and a "one entry per day" logic to build consistency.
- **🏷️ Smart Metadata:** Comprehensive tagging system and multi-emoji mood tracking (Primary & Secondary).
- **🔍 Powerful Filtering:** Search through years of entries by text, date ranges, moods, or tags.
- **📑 Database Pagination:** High-speed performance regardless of entry count.
- **🌑 Global Theming:** Full support for Dark Mode and Light Mode with preference persistence.
- **📅 Visual Timeline:** Historical browsing with specialized UI for mood colors and categories.
- **📁 Data Portability:** Export your journal entries to high-quality PDF documents based on selected date ranges.

## 🛠️ Technical Stack

- **Framework:** .NET MAUI (Multi-platform App UI)
- **Database:** SQLite (Local & Offline-first)
- **Architecture:** MVVM (Model-View-ViewModel) Pattern
- **Library (Charts):** LiveChartsCore (SkiaSharp)
- **Library (Markdown):** Markdig
- **UI Toolkit:** CommunityToolkit.Mvvm

## 🚀 Getting Started

### Prerequisites
- Visual Studio 2022 with **.NET MAUI workload** installed.
- .NET 8.0 SDK or higher.

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/sagar2346/Daymark_Sagar-Dahal.git
   ```
2. Open the solution file `DailyJournalApp.sln` in Visual Studio.
3. Restore NuGet packages.
4. Set `DailyJournalApp` as the startup project.
5. Select your target platform (Windows, Android, or iOS) and press **F5** to Run.

## 🧪 Test Coverage confirmed
The application is ready for the following test cases:
- [x] Test 4.1: Toggle Dark/Light mode working or not.
- [x] Test 4.2: Export function (PDF Generation) works correctly.
- [x] Test 4.4: Login portal performance and verification.
- [x] Test 4.5: User ability to change login credentials.
- [x] Test 4.6: Markdown and Rich Text Rendering logic.
- [x] Test 4.7: Mood Tracking (Primary and Secondary emotions).
- [x] Test 4.8: Tagging System and metadata management.
- [x] Test 4.9: Advanced Search & Filters (Text, Date, Mood, Tag).
- [x] Test 4.10: Streak Tracking & "Daily Limit" enforcement.
- [x] Test 4.11: Calendar Navigation for date-based loading.
- [x] Test 4.12: Analytics & Insights generation.
- [x] Test 4.13: Paginated Journal View for performance.


