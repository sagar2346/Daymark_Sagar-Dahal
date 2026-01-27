# DayMark — Premium Personal Journaling System 📝

A high-performance, secure, and feature-rich cross-platform journaling application built with **.NET MAUI** and **SQLite**. Designed for users who value privacy, rich-text expression, and data-driven personal growth.

## 🌟 Key Features

- **🔐 Secure Authentication:** Password-protected entries with setup and login modes.
- **✍️ Markdown Editor:** Rich-text support with live HTML preview rendering.
- **📊 Advanced Analytics:** Interactive charts for mood distribution, word count trends, and time patterns.
- **🔥 Persistence & Habits:** Automatic journaling streak tracking to build long-term consistency.
- **🏷️ Smart Metadata:** Comprehensive tagging system and multi-emoji mood tracking (Primary & Secondary).
- **🔍 Powerful Filtering:** Search through years of entries by text, date ranges, moods, or tags.
- **📑 Database Pagination:** High-speed performance regardless of entry count.
- **🌑 Global Theming:** Full support for Dark Mode and Light Mode with preference persistence.
- **📅 Visual Timeline:** Historical browsing with specialized UI for mood colors and categories.
- **🗓️ Dynamic Calendar:** Interactive date selection for precise journaling and historical retrieval.
- **🛑 Smart Limit System:** Quality control logic preventing more than one entry per day for focused writing.
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
- [x] : Toggle Dark/Light mode .
- [x] : Export function (PDF Generation).
- [x] : Login portal performance and verification.
- [x] : User ability to change login credentials.
- [x] : Markdown and Rich Text Rendering logic.
- [x] : Mood Tracking (Primary and Secondary emotions).
- [x] : Tagging System and metadata management.
- [x] : Advanced Search & Filters (Text, Date, Mood, Tag).
- [x] : Streak Tracking & "Daily Limit" enforcement.
- [x] : Calendar Navigation for date-based loading.
- [x] : Analytics & Insights generation.
- [x] : Paginated Journal View for performance.
