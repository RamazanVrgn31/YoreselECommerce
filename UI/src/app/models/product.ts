export interface Product {
    id: number;
    categoryId: number;
    productName: string;
    price: number;        // Backend: Price (decimal)
    unitsInStock: number; // Backend: UnitsInStock
    description: string;  // Backend: Description
    imageUrl: string;     // Backend: ImageUrl
    isActive: boolean;
}

// Backend'den gelen genel yanıt kalıbı (Result)
export interface DataResult<T> {
    success: boolean;
    message: string;
    data: T;
}
