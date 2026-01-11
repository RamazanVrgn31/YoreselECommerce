export interface BasketItem {
    productId: number;
    productName: string;
    quantity: number;
    price: number;     // Backend'deki 'Price' (Satış Fiyatı)
    unitPrice: number; // Backend'deki 'UnitPrice'
}

export interface BasketDto {
    items: BasketItem[]; // C#'taki List<BasketItemDto> Items
    totalPrice: number; // C#'taki TotalPrice
}

export interface AddBasketItemDto {
    productId: number;
    quantity: number;
}