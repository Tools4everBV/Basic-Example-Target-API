# Change Log

All notable changes to this project will be documented in this file. The format is based on [Keep a Changelog](https://keepachangelog.com), and this project adheres to [Semantic Versioning](https://semver.org).

## [2.1.2] - 19-02-2026

### Changed

- Updated database migrations and `POST /api/users` to prevent creating users with duplicate `externalId` values.
  See issue: https://github.com/Tools4everBV/Basic-Example-Target-API/issues/6

- Updated migrations, `ApplicationDbContext`, and data models to support cascading deletes.
  When a user is deleted, related authorizations are now automatically removed.
  See issue: https://github.com/Tools4everBV/Basic-Example-Target-API/issues/5

- Added validation to ensure a user exists before creating a new authorization.
  Prevents authorizations from being created for non-existing users.
  See issue: https://github.com/Tools4everBV/Basic-Example-Target-API/issues/2

- Updated README.md

### Added

- Introduced the Installer / API Manager to manage the API lifecycle (install, update, start, stop, uninstall). See issue: https://github.com/Tools4everBV/Basic-Example-Target-API/issues/3
- Application icon.

## [2.1.1] - 02-10-2025

### Added

- Added API controller for versioning.

## [2.0.1] - 25-09-2025

### Changed

- Solved issue where authorizations didn't return the internal `id`. See issue: https://github.com/Tools4everBV/Basic-Example-Target-API/issues/7

## [2.0.0] - 23-09-2025

### Added

- oAuth authentication using _ClientId_ and _ClientSecret_.
- Builtin API examples for all API calls.
- API call to retrieve authorization for all users to support the HelloID import entitlement feature.

### Changed
- Splitted the _Roles_ endpoint into _Authorizations_ and _Roles_ for a more clear distinction.
- Renamed all endpoints to lowercase.
- Changed sequence of endpoints in swagger interface.

## [1.0.0] - 04-11-2022

This is the first official release of the _Basic-Example-Target-API_.

### Added

### Changed

### Deprecated

### Removed