# Testing Notes

## 1. Test Results

- Total Tests: 20
- Passed: 20
- Failed: 0
- Skipped: 0

All tests passed successfully.

## 2. Code Coverage

| Metric | Coverage |
|---|---:|
| Line Coverage | 90.5% |
| Branch Coverage | 83.3% |
| Method Coverage | 88.8% |

### Coverage by Project

| Project | Line Coverage |
|---|---:|
| Domain | 100% |
| Application | 95.8% |
| Infrastructure | 0% |

The required 80% line coverage for both Domain and Application was achieved.

## 3. Testing Libraries

- **Moq:** Used to create mock dependencies such as repositories, inventory services, and notification services. This allows testing CheckoutService in isolation.
- **AutoFixture:** Used to automatically generate test data and reduce repetitive object creation.
- **Bogus:** Used to generate realistic fake customer data with a fixed seed to ensure deterministic test results.
- **FluentAssertions:** Used to write readable and expressive assertions.
- **Verify:** Used for snapshot testing to ensure checkout receipts match approved snapshots.

## 4. Business Logic Question

A customer purchasing products worth 999.99 receives no discount, while a customer purchasing products worth 1000 receives a 100 discount.

This creates a sharp difference between two nearly identical orders.

The business should evaluate whether this threshold is fair and appropriate. Possible alternatives include:
- Using a percentage-based discount.
- Introducing multiple discount tiers.
- Reviewing the minimum purchase threshold.

However, the final decision depends on the business strategy and profit margins.

## 5. Bugs Discovered

No confirmed production bugs were discovered during the implemented tests.