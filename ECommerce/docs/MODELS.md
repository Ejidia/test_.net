# E-Commerce Models Structure

## Overview
This document describes the data models for the e-commerce application.

## Model Relationships

```
User (1) -----> (Many) Orders
User (1) -----> (Many) Reviews
User (1) -----> (Many) CartItems
User (1) -----> (1) Address (Shipping)

Category (1) -----> (Many) Products
Category (1) -----> (Many) SubCategories (self-reference)

Product (1) -----> (Many) ProductImages
Product (1) -----> (Many) Reviews
Product (1) -----> (Many) OrderItems
Product (1) -----> (Many) CartItems
Product (Many) -----> (1) Category

Order (1) -----> (Many) OrderItems
Order (1) -----> (1) Payment
Order (1) -----> (1) Address (Shipping)
Order (Many) -----> (1) User

OrderItem (Many) -----> (1) Order
OrderItem (Many) -----> (1) Product

CartItem (Many) -----> (1) User
CartItem (Many) -----> (1) Product

Review (Many) -----> (1) Product
Review (Many) -----> (1) User

Payment (1) -----> (1) Order
```

## Models Description

### 1. User
**Purpose:** Represents customers and admins
- **Key Fields:** Email, PasswordHash, FirstName, LastName, Role
- **Relationships:** Has many Orders, Reviews, CartItems; Has one Address
- **Role Types:** Customer, Admin, Seller

### 2. Category
**Purpose:** Product categorization with hierarchy support
- **Key Fields:** Name, Description, ParentCategoryId
- **Relationships:** Has many Products, SubCategories; Belongs to ParentCategory
- **Features:** Supports nested categories (e.g., Electronics > Phones > Smartphones)

### 3. Product
**Purpose:** Items available for purchase
- **Key Fields:** Name, Description, Price, DiscountPrice, StockQuantity, SKU
- **Relationships:** Belongs to Category; Has many Images, Reviews, OrderItems, CartItems
- **Computed:** AverageRating, ReviewCount

### 4. ProductImage
**Purpose:** Multiple images per product
- **Key Fields:** ImageUrl, IsPrimary, DisplayOrder
- **Relationships:** Belongs to Product
- **Features:** Primary image flag, custom ordering

### 5. Order
**Purpose:** Customer purchase records
- **Key Fields:** OrderNumber, SubTotal, ShippingCost, Tax, Total, Status
- **Relationships:** Belongs to User; Has many OrderItems; Has one Payment, Address
- **Status Flow:** Pending → Processing → Shipped → Delivered
- **Tracking:** TrackingNumber, ShippedDate, DeliveredDate

### 6. OrderItem
**Purpose:** Individual products within an order
- **Key Fields:** Quantity, UnitPrice, Discount, Total
- **Relationships:** Belongs to Order and Product
- **Purpose:** Captures price at time of purchase (price history)

### 7. CartItem
**Purpose:** Shopping cart functionality
- **Key Fields:** Quantity, AddedAt
- **Relationships:** Belongs to User and Product
- **Computed:** SubTotal (considers discount price)

### 8. Payment
**Purpose:** Payment transaction records
- **Key Fields:** Amount, PaymentMethod, Status, TransactionId
- **Relationships:** Belongs to Order
- **Methods:** CreditCard, PayPal, Stripe, etc.
- **Status:** Pending, Completed, Failed, Refunded

### 9. Address
**Purpose:** Shipping and billing addresses
- **Key Fields:** FullName, AddressLine1, City, State, PostalCode, Country
- **Relationships:** Can belong to User (saved address) or Order (shipping address)
- **Features:** IsDefault flag for user's primary address

### 10. Review
**Purpose:** Product ratings and feedback
- **Key Fields:** Rating (1-5), Title, Comment
- **Relationships:** Belongs to Product and User
- **Features:** IsVerifiedPurchase flag, timestamps

## Enums

### OrderStatus
- Pending: Order placed, awaiting processing
- Processing: Order being prepared
- Shipped: Order dispatched
- Delivered: Order received by customer
- Cancelled: Order cancelled
- Refunded: Payment returned

### UserRole
- Customer: Regular buyer
- Admin: System administrator
- Seller: Product vendor

### PaymentStatus
- Pending: Payment initiated
- Completed: Payment successful
- Failed: Payment unsuccessful
- Refunded: Money returned

## Key Design Patterns

1. **Soft Delete:** IsActive flags instead of hard deletes
2. **Audit Trail:** CreatedAt, UpdatedAt timestamps
3. **Price History:** OrderItem stores price at purchase time
4. **Computed Properties:** AverageRating, SubTotal calculated on-the-fly
5. **Hierarchical Data:** Categories support parent-child relationships
6. **Flexible Addressing:** Address can be linked to User or Order

## Database Considerations

- **Indexes:** Email (unique), SKU (unique), OrderNumber (unique)
- **Constraints:** Rating (1-5), StockQuantity (>= 0), Price (> 0)
- **Cascading:** Deleting Product should handle related OrderItems, Reviews
- **Transactions:** Order creation should be atomic (Order + OrderItems + Payment)
